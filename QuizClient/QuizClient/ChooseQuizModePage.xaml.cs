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
        private readonly Frame _mainFrame;
        private readonly string _jwtToken;

       public ChooseQuizModePage(Frame mainFrame, string jwtToken)
            {
                InitializeComponent();
                _mainFrame = mainFrame;
                _jwtToken = jwtToken;
                Utils.JwtUtils.ValidateDocenteRole(_jwtToken, this.NavigationService, this);
        }

        private void ManualeButton_Click(object sender, RoutedEventArgs e)
        {
            // Naviga alla pagina di gestione dei quiz - (Il quiz viene passato vuoto)
            _mainFrame.Navigate(new QuizManagerPage(_mainFrame, _jwtToken, new Models.Quiz()));
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            // Naviga alla pagina di creazione del quiz
            _mainFrame.Navigate(new CreateQuizPage(_mainFrame, _jwtToken, Mode.Default));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
            
        }
    }
}
