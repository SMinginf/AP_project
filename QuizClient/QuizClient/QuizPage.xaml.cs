using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using QuizClient.Models;
using QuizClient.Services;
using QuizClient.Utils;

namespace QuizClient
{
    public partial class QuizPage : Page
    {
        private DispatcherTimer? _timer;

        private readonly Quiz _quiz;
        private int _currentIndex = 0;
        private int _secondsElapsed = 0;
        private readonly List<int?> _risposteUtente;
        
        private string OrarioCreazione { get; set; }
        
        private readonly QuizService _quizService;
        private readonly string _jwtToken;

        public QuizPage(Quiz quiz, string jwt)
        {
            InitializeComponent();
            _jwtToken = jwt;
            _quizService = new QuizService(_jwtToken);
            
            
            _quiz = quiz;            
            _risposteUtente = new List<int?>(new int?[quiz.Quesiti.Count]);    
            QuizTitleText.Text = quiz.Titolo;

            OrarioCreazione = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            StartTimer();
            MostraQuesito();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _secondsElapsed++;
                var ts = TimeSpan.FromSeconds(_secondsElapsed);
                TimerText.Text = $"Tempo: {ts:hh\\:mm\\:ss}";
            };
            _timer.Start();
        }

        private void MostraQuesito()
        {
            if (_quiz.Quesiti.Count == 0) return;

            var q = _quiz.Quesiti[_currentIndex];
            QuesitoText.Text = q.Testo;

            OpzioneAButton.Content = q.OpzioneA;
            OpzioneBButton.Content = q.OpzioneB;
            OpzioneCButton.Content = q.OpzioneC;
            OpzioneDButton.Content = q.OpzioneD;

            ResetButtonStyle(OpzioneAButton);
            ResetButtonStyle(OpzioneBButton);
            ResetButtonStyle(OpzioneCButton);
            ResetButtonStyle(OpzioneDButton);

            int? risposta = _risposteUtente[_currentIndex];
            if (risposta.HasValue)
            {
                EvidenziaRisposta(risposta.Value, q.OpCorretta);
                DisabilitaBottoniRisposta(true);
            }
            else
            {
                DisabilitaBottoniRisposta(false);
            }

            PrevQuesitoButton.IsEnabled = _currentIndex > 0;
            NextQuesitoButton.IsEnabled = _currentIndex < _quiz.Quesiti.Count - 1;

        }

        private void Risposta_Click(object sender, RoutedEventArgs e)
        {
            if (_quiz.Quesiti.Count == 0) return;
            var q = _quiz.Quesiti[_currentIndex];

            // operatore ternario annidato per determinare l'indice della risposta selezionata
            int rispostaUtente = (sender == OpzioneAButton) ? 0 :
                                 (sender == OpzioneBButton) ? 1 :
                                 (sender == OpzioneCButton) ? 2 : 3;

            _risposteUtente[_currentIndex] = rispostaUtente;

            EvidenziaRisposta(rispostaUtente, q.OpCorretta);
            DisabilitaBottoniRisposta(true);
        }

        private void EvidenziaRisposta(int rispostaUtente, int rispostaCorretta)
        {
            // Verde se corretta, rosso se sbagliata
            Button[] bottoni = { OpzioneAButton, OpzioneBButton, OpzioneCButton, OpzioneDButton };
            for (int i = 0; i < bottoni.Length; i++)
            {
                if (i == rispostaUtente)
                {
                    bottoni[i].BorderBrush = (rispostaUtente == rispostaCorretta) ? Brushes.Green : Brushes.Red;
                    bottoni[i].BorderThickness = new Thickness(3);
                }
                else if (i == rispostaCorretta)
                {
                    bottoni[i].BorderBrush = Brushes.Green;
                    bottoni[i].BorderThickness = new Thickness(3);
                }
                else
                {
                    ResetButtonStyle(bottoni[i]);
                }
            }
        }

        private void ResetButtonStyle(Button btn)
        {
            btn.ClearValue(Button.BorderBrushProperty);
            btn.ClearValue(Button.BorderThicknessProperty);
            btn.IsEnabled = true;
        }

        private void DisabilitaBottoniRisposta(bool disabilita)
        {
            OpzioneAButton.IsEnabled = !disabilita;
            OpzioneBButton.IsEnabled = !disabilita;
            OpzioneCButton.IsEnabled = !disabilita;
            OpzioneDButton.IsEnabled = !disabilita;
        }

        private void PrevQuesito_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                MostraQuesito();
            }
        }

        private void NextQuesito_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _quiz.Quesiti.Count - 1)
            {
                _currentIndex++;
                MostraQuesito();
            }
        }

        private async void EndQuiz_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            var ts = TimeSpan.FromSeconds(_secondsElapsed);
            string durata = ts.ToString(@"hh\:mm\:ss");

            int corrette = 0;
            int sbagliate = 0;
            for (int i = 0; i < _quiz.Quesiti.Count; i++)
            {
                // Assicurati che _risposteUtente[i] non sia null prima di accedere al suo valore
                if (_risposteUtente != null && i < _risposteUtente.Count && _risposteUtente[i] is int risposta )               
                {
                    if (risposta == _quiz.Quesiti[i].OpCorretta)
                        corrette++;
                    else
                        sbagliate++;
                }
            }

            if (_quiz.AiGenerated) {
                MessageBox.Show($"Quiz terminato!\nRisposte corrette: {corrette} su {_quiz.Quesiti.Count}\nTempo: {_secondsElapsed} secondi");
                return;
            }

            var result = await _quizService.StoreQuizAsync(_quiz, corrette, sbagliate, OrarioCreazione, durata, _risposteUtente);
            if (result != null && result.Success)
            {
                MessageBox.Show($"Quiz terminato!\nRisposte corrette: {corrette} su {_quiz.Quesiti.Count}\nTempo: {_secondsElapsed} secondi");

                // Naviga alla pagina Lobby
                //var lobbyPage = new LobbyPage(_jwtToken);
                //NavigationService?.Navigate(lobbyPage);


            }
            else
            {
                MessageBox.Show(result?.ErrorMessage ?? "Errore nell'invio del quiz al server.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CancelQuiz_Click(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
