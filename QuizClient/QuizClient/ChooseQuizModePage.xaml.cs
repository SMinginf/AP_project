using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using QuizClient.Models;

namespace QuizClient
{
    public partial class ChooseQuizModePage : Page
    {
        private readonly string _jwtToken;

       public ChooseQuizModePage(string jwtToken)
            {
                InitializeComponent();
                _jwtToken = jwtToken;
                Utils.JwtUtils.ValidateDocenteRole(_jwtToken, this.NavigationService, this);
        }

        private void ManualeButton_Click(object sender, RoutedEventArgs e)
        {
            // Naviga alla pagina di gestione dei quiz - (Il quiz viene passato vuoto)
            NavigationService.Navigate(new QuizManagerPage(_jwtToken, new Models.Quiz()));
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            // Naviga alla pagina di creazione del quiz
            NavigationService.Navigate(new CreateQuizPage(_jwtToken, Mode.Default));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LobbyPage(_jwtToken));
            
        }
    }
}
