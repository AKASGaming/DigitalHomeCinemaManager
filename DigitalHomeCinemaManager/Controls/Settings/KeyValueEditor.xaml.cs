

namespace DigitalHomeCinemaManager.Controls.Settings
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for KeyValueEditor.xaml
    /// </summary>
    public partial class KeyValueEditor : Window
    {

        public KeyValueEditor(string name, string value)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(name)) {
                this.key.Text = name;
            }
            if (!string.IsNullOrEmpty(value)) {
                this.value.Text = value;
            }

        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public string Key
        {
            get { return this.key.Text; }
        }

        public string Value
        {
            get { return this.value.Text; }
        }

    }

}
