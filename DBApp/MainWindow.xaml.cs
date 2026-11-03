using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using SQLitePCL;
using System.Collections.ObjectModel; // made me do this for observablecollection
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private enum SortMode { Name, Age }
     
        private readonly PeopleContext database = new();
        private SortMode currentSort = SortMode.Name;

        public ObservableCollection <Person> People { get; private set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            Batteries.Init(); // <---- without this it doesn't work so don't touch this thing

            database.Database.EnsureCreated();
            database.People.Load();

            DataContext = this;
            RefreshPeople();
        }


        private void RefreshPeople()
        {
            Person? previouslySelected = PeopleGrid.SelectedItem as Person;
            string searchText = SearchTextBox.Text;

            IEnumerable<Person> query = database.People.Local;

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(person =>
                    person.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    person.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (currentSort == SortMode.Name)
            {
                query = query.OrderBy(person => person.LastName).ThenBy(person => person.FirstName);
                SortStatusTextBlock.Text = "Sorting by: Last, First name";
            }
            else
            {
                query = query.OrderBy(person => person.Age);
                SortStatusTextBlock.Text = "Sorting by: Age";
            }

            var results = query.ToList();

            People.Clear();
            foreach (Person person in results)
            {
                People.Add(person);
            }

            if (previouslySelected != null && People.Contains(previouslySelected))
            {
                PeopleGrid.SelectedItem = previouslySelected;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshPeople(); 
        }

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {
            currentSort = SortMode.Name;
            RefreshPeople();
        }

        private void SortByAge_Click(object sender, RoutedEventArgs e)
        {
            currentSort = SortMode.Age;
            RefreshPeople();
        }

        private void AddPerson_Click(object sender, RoutedEventArgs e)
        {

            SearchTextBox.Text = "";

            Person person = new Person { FirstName = "New", LastName = "Person", Age = 18 };
            database.People.Add(person);
            RefreshPeople();

            PeopleGrid.SelectedItem = person;
            PeopleGrid.ScrollIntoView(person);

            FirstNameTextBox.Focus();
            FirstNameTextBox.SelectAll();
        }

        private void DeletePerson_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleGrid.SelectedItem is Person person)
            {
                database.People.Remove(person);
                RefreshPeople();
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            database.SaveChanges();
        }
    }
}