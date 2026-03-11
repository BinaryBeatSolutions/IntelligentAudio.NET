using Avalonia.Controls;


namespace IntelligentAudio.Dashboard
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Vi sätter DataContext manuellt här för att verifiera kopplingen
        public MainWindow(NexusDashboardViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}