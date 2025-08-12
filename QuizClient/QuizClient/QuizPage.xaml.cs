using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Threading.Tasks;
=======
using System.Linq;
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
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
<<<<<<< HEAD
        private DispatcherTimer? _timer;

        private readonly Quiz _quiz;
        private int _currentIndex = 0;
        private int _secondsElapsed = 0;
        private readonly List<int?> _risposteUtente;
        
        private string OrarioCreazione { get; set; }
        
        private readonly QuizService _quizService;
        private readonly string _jwtToken;
=======
        private readonly Quiz _quiz;
        private readonly string _jwtToken;
        private readonly QuizService _quizService;
        private DispatcherTimer _timer;
        private int _secondsElapsed = 0;

        private readonly List<int?> _risposteUtente;
        private readonly List<StackPanel> _quesitiUI = new();

        private string OrarioCreazione { get; set; }
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba

        public QuizPage(Quiz quiz, string jwt)
        {
            InitializeComponent();
<<<<<<< HEAD
            _jwtToken = jwt;
            _quizService = new QuizService(_jwtToken);
            
            
            _quiz = quiz;            
            _risposteUtente = new List<int?>(new int?[quiz.Quesiti.Count]);    
            QuizTitleText.Text = quiz.Titolo;

            OrarioCreazione = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            StartTimer();
            MostraQuesito();
=======
            _timer = new DispatcherTimer();
            _quiz = quiz;
            _jwtToken = jwt;
            _quizService = new QuizService(_jwtToken);
            _risposteUtente = new List<int?>(new int?[quiz.Quesiti.Count]);
            OrarioCreazione = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            QuizTitleText.Text = "Test in";

            StartTimer();
            CreaDomande();
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        }

        private void StartTimer()
        {
<<<<<<< HEAD
            _timer = new DispatcherTimer();
=======
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _secondsElapsed++;
                var ts = TimeSpan.FromSeconds(_secondsElapsed);
                TimerText.Text = $"Tempo: {ts:hh\\:mm\\:ss}";
            };
            _timer.Start();
        }

<<<<<<< HEAD
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
=======
        private void CreaDomande()
        {
            for (int i = 0; i < _quiz.Quesiti.Count; i++)
            {
                var q = _quiz.Quesiti[i];

                var contenitore = new Border
                {
                    Style = (Style)FindResource("CardStyle"),
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var domandaPanel = new StackPanel
                {
                    Tag = i
                };

                domandaPanel.Children.Add(new TextBlock
                {
                    Text = $"{i + 1}. {q.Testo}",
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var opzioni = new[] { q.OpzioneA, q.OpzioneB, q.OpzioneC, q.OpzioneD };

                for (int j = 0; j < 4; j++)
                {
                    var radio = new RadioButton
                    {
                        Content = opzioni[j],
                        FontSize = 16,
                        Margin = new Thickness(10, 5, 0, 5),
                        Tag = j,
                        GroupName = $"GruppoDomanda{i}"
                    };

                    int domandaIndex = i;
                    int opzioneIndex = j;

                    radio.Checked += (s, e) =>
                    {
                        _risposteUtente[domandaIndex] = opzioneIndex;
                    };

                    domandaPanel.Children.Add(radio);
                }

                var feedbackText = new TextBlock
                {
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 0, 0),
                    Foreground = Brushes.Gray,
                    Visibility = Visibility.Collapsed,
                    Tag = "Feedback"
                };
                domandaPanel.Children.Add(feedbackText);

                contenitore.Child = domandaPanel;
                DomandeContainer.Children.Add(contenitore);
                _quesitiUI.Add(domandaPanel);
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            }
        }

        private async void EndQuiz_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            _timer?.Stop();
=======
            _timer.Stop();
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
            var ts = TimeSpan.FromSeconds(_secondsElapsed);
            string durata = ts.ToString(@"hh\:mm\:ss");

            int corrette = 0;
            int sbagliate = 0;
<<<<<<< HEAD
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

=======

            for (int i = 0; i < _quiz.Quesiti.Count; i++)
            {
                var rispostaUtente = _risposteUtente[i];
                var corretta = _quiz.Quesiti[i].OpCorretta;

                StackPanel domandaPanel = _quesitiUI[i];
                TextBlock? feedbackText = null;

                for (int j = 1; j <= 4; j++)
                {
                    var radio = (RadioButton)domandaPanel.Children[j];
                    int indice = (int)radio.Tag;

                    if (indice == corretta)
                    {
                        radio.BorderBrush = Brushes.Green;
                        radio.BorderThickness = new Thickness(2);
                    }

                    if (rispostaUtente.HasValue && indice == rispostaUtente && rispostaUtente != corretta)
                    {
                        radio.BorderBrush = Brushes.Red;
                        radio.BorderThickness = new Thickness(2);
                    }

                    radio.IsEnabled = false;
                }

                foreach (var child in domandaPanel.Children)
                {
                    if (child is TextBlock tb && tb.Tag?.ToString() == "Feedback")
                    {
                        feedbackText = tb;
                        break;
                    }
                }

                if (feedbackText != null)
                {
                    feedbackText.Visibility = Visibility.Visible;
                    string rispostaEsatta = GetTestoRisposta(_quiz.Quesiti[i], corretta);


                    if (!rispostaUtente.HasValue)
                    {
                        feedbackText.Text = $"Nessuna risposta data. Risposta corretta: {rispostaEsatta}";
                        feedbackText.Foreground = Brushes.OrangeRed;
                    }
                    else if (rispostaUtente == corretta)
                    {
                        feedbackText.Text = "✅ Risposta corretta.";
                        feedbackText.Foreground = Brushes.Green;
                    }
                    else
                    {
                        feedbackText.Text = $"❌ Risposta errata. Corretta: {rispostaEsatta}";
                        feedbackText.Foreground = Brushes.Red;
                    }
                }

                if (rispostaUtente.HasValue)
                {
                    if (rispostaUtente.Value == corretta) corrette++;
                    else sbagliate++;
                }
            }

            var res = await _quizService.StoreQuizAsync(_quiz, corrette, sbagliate, OrarioCreazione, durata, _risposteUtente);
            if (!res.Success)
            {
                MessageBox.Show($"Errore durante il salvataggio del quiz: {res.ErrorMessage}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FeedbackTitle.Text = "Quiz completato!";
            FeedbackDetails.Text = $"Risposte corrette: {corrette} su {_quiz.Quesiti.Count}\nTempo impiegato: {durata}";
            QuizFeedbackPanel.Visibility = Visibility.Visible;
            EndQuizButton.Visibility = Visibility.Collapsed;
            CancelQuizButton.Visibility = Visibility.Collapsed;
        }

        private void GoToLobby_Click(object sender, RoutedEventArgs e)
        {
            var lobby = new LobbyPage(_jwtToken);
            NavigationService?.Navigate(lobby);
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
        }

        private void CancelQuiz_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
            _timer?.Stop();
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
=======
            _timer.Stop();
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private string GetTestoRisposta(Quesito q, int index)
        {
            return index switch
            {
                0 => q.OpzioneA,
                1 => q.OpzioneB,
                2 => q.OpzioneC,
                3 => q.OpzioneD,
                _ => "Risposta sconosciuta"
            };
        }

    }
}
>>>>>>> a8b552f97ebfc43b0b057ddd5cbe7c374024d6ba
