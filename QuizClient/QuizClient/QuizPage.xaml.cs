/*
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using QuizClient.Models;
using QuizClient.Services;


namespace QuizClient
{
    public partial class QuizPage : Page
    {
        private readonly Quiz _quiz;
        private readonly string _jwtToken;
        private readonly QuizService _quizService;
        private DispatcherTimer _timer;
        private int _secondsElapsed = 0;

        private readonly List<int?> _risposteUtente;
        private readonly List<StackPanel> _quesitiUI = new();

        private string OrarioCreazione { get; set; }

        public QuizPage(Quiz quiz, string jwt)
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _quiz = quiz;
            _jwtToken = jwt;
            _quizService = new QuizService(_jwtToken);
            _risposteUtente = new List<int?>(new int?[quiz.Quesiti.Count]);
            OrarioCreazione = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            QuizTitleText.Text = "Test in";

            StartTimer();
            CreaDomande();
        }

        private void StartTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                _secondsElapsed++;
                var ts = TimeSpan.FromSeconds(_secondsElapsed);
                TimerText.Text = $"Tempo: {ts:hh\\:mm\\:ss}";
            };
            _timer.Start();
        }

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
            }
        }

        private async void EndQuiz_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            var ts = TimeSpan.FromSeconds(_secondsElapsed);
            string durata = ts.ToString(@"hh\:mm\:ss");

            int corrette = 0;
            int sbagliate = 0;

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
        }

        private void CancelQuiz_Click(object sender, RoutedEventArgs e)
        {
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
} */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using QuizClient.Models;
using QuizClient.Services;

namespace QuizClient
{
    public partial class QuizPage : Page, IDisposable
    {
        private readonly Quiz _quiz;
        private readonly string _jwtToken;
        private readonly QuizService _quizService;
        private readonly DispatcherTimer _timer;
        private readonly List<int?> _risposteUtente;
        private readonly List<StackPanel> _quesitiUI = new();

        private int _secondsElapsed;
        private bool _disposed;

        private string OrarioCreazione { get; }

        public QuizPage(Quiz quiz, string jwtToken)
        {
            InitializeComponent();

            _quiz = quiz;
            _jwtToken = jwtToken;
            _quizService = new QuizService(jwtToken);
            _risposteUtente = Enumerable.Repeat<int?>(null, quiz.Quesiti.Count).ToList();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            OrarioCreazione = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            QuizTitleText.Text = "Test in corso";

            _timer.Tick += Timer_Tick;
            _timer.Start();

            CreaDomande();
        }

        // --------------------- //
        //     GESTIONE TIMER    //
        // --------------------- //

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _secondsElapsed++;
            var ts = TimeSpan.FromSeconds(_secondsElapsed);
            TimerText.Text = $"Tempo: {ts:hh\\:mm\\:ss}";
        }

        // --------------------- //
        //   CREAZIONE DOMANDE   //
        // --------------------- //

        private void CreaDomande()
        {
            for (int i = 0; i < _quiz.Quesiti.Count; i++)
            {
                var q = _quiz.Quesiti[i];

                // Card contenitore
                var contenitore = new Border
                {
                    Style = (Style)FindResource("CardStyle"),
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var domandaPanel = new StackPanel { Tag = i };

                // Testo domanda
                domandaPanel.Children.Add(new TextBlock
                {
                    Text = $"{i + 1}. {q.Testo}",
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                // Opzioni di risposta
                var opzioni = new[] { q.OpzioneA, q.OpzioneB, q.OpzioneC, q.OpzioneD };

                for (int j = 0; j < opzioni.Length; j++)
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

                    // Salva la risposta selezionata
                    radio.Checked += (_, _) => _risposteUtente[domandaIndex] = opzioneIndex;

                    domandaPanel.Children.Add(radio);
                }

                // Feedback nascosto inizialmente
                domandaPanel.Children.Add(new TextBlock
                {
                    FontSize = 14,
                    Margin = new Thickness(10, 10, 0, 0),
                    Foreground = Brushes.Gray,
                    Visibility = Visibility.Collapsed,
                    Tag = "Feedback"
                });

                contenitore.Child = domandaPanel;
                DomandeContainer.Children.Add(contenitore);
                _quesitiUI.Add(domandaPanel);
            }
        }

        // --------------------- //
        //  GESTIONE FINE QUIZ   //
        // --------------------- //

        private async void EndQuiz_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            var durata = TimeSpan.FromSeconds(_secondsElapsed).ToString(@"hh\:mm\:ss");

            int corrette = 0, sbagliate = 0;

            for (int i = 0; i < _quiz.Quesiti.Count; i++)
            {
                var domandaPanel = _quesitiUI[i];
                var rispostaUtente = _risposteUtente[i];
                var corretta = _quiz.Quesiti[i].OpCorretta;

                EvidenziaRisposte(domandaPanel, rispostaUtente, corretta, out var feedbackText);

                if (rispostaUtente.HasValue)
                {
                    if (rispostaUtente == corretta) corrette++;
                    else sbagliate++;
                }
            }

            // Salva quiz completato
            var result = await _quizService.StoreQuizAsync(_quiz, corrette, sbagliate, OrarioCreazione, durata, _risposteUtente);
            if (!result.Success)
            {
                MessageBox.Show($"Errore durante il salvataggio del quiz: {result.ErrorMessage}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FeedbackTitle.Text = "Quiz completato!";
            FeedbackDetails.Text = $"Risposte corrette: {corrette} su {_quiz.Quesiti.Count}\nTempo impiegato: {durata}";

            QuizFeedbackPanel.Visibility = Visibility.Visible;
            EndQuizButton.Visibility = Visibility.Collapsed;
            CancelQuizButton.Visibility = Visibility.Collapsed;
        }

        private void EvidenziaRisposte(StackPanel domandaPanel, int? rispostaUtente, int corretta, out TextBlock? feedbackText)
        {
            feedbackText = null;

            // Scorri i RadioButton (da 1 a 4) e mostra i bordi
            foreach (var child in domandaPanel.Children.OfType<RadioButton>())
            {
                int indice = (int)child.Tag;

                if (indice == corretta)
                {
                    child.BorderBrush = Brushes.Green;
                    child.BorderThickness = new Thickness(2);
                }
                else if (rispostaUtente.HasValue && indice == rispostaUtente && rispostaUtente != corretta)
                {
                    child.BorderBrush = Brushes.Red;
                    child.BorderThickness = new Thickness(2);
                }

                child.IsEnabled = false;
            }

            // Trova TextBlock con Tag="Feedback"
            feedbackText = domandaPanel.Children
                .OfType<TextBlock>()
                .FirstOrDefault(tb => tb.Tag?.ToString() == "Feedback");

            if (feedbackText == null)
                return;

            feedbackText.Visibility = Visibility.Visible;

            string rispostaEsatta = GetTestoRisposta(_quiz.Quesiti[(int)domandaPanel.Tag], corretta);

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

        // --------------------- //
        //     NAVIGAZIONE UI    //
        // --------------------- //

        private void GoToLobby_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LobbyPage(_jwtToken));
        }

        private void CancelQuiz_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            if (NavigationService.CanGoBack == true)
                NavigationService.GoBack();
        }

        private static string GetTestoRisposta(Quesito q, int index) => index switch
        {
            0 => q.OpzioneA,
            1 => q.OpzioneB,
            2 => q.OpzioneC,
            3 => q.OpzioneD,
            _ => "Risposta sconosciuta"
        };

        // --------------------- //
        //    DISPOSING RISORSE  //
        // --------------------- //

        public void Dispose()
        {
            if (_disposed) return;

            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            _quizService.Dispose();

            _disposed = true;
            GC.SuppressFinalize(this);
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Chiama il metodo Dispose() quando la pagina non è più visualizzata
            (this as IDisposable)?.Dispose();
        }
    }
}
