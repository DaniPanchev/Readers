using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows;
using Library; 
using Microsoft.Data.SqlClient;
using static Library.Program;

namespace WpfLibraryDb
{
    public partial class MainWindow : Window
    {
        private Library library;
        private const string ConnectionString = "Server=desktop-gjv8d7s\\\\SQLEXPRESS;Database=localdb;Trusted_Connection=true;TrustServerCertificate=True";
        public MainWindow()
        {
            InitializeComponent();
            library = new Library();
            Application.Current.Exit += OnApplicationExit;
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            SaveBooksToDb();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            SaveBooksToDb();
            MessageBox.Show("Books saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnLoadClicked(object sender, RoutedEventArgs e)
        {
            LoadBooksFromDb();
            MessageBox.Show("Books loaded successfully.", "Load", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveBooksToDb()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    foreach (var book in library.GetAllBooks())
                    {
                        string query = "INSERT INTO Books (Title, AuthorName, AuthorEmail, AuthorGender, Price) VALUES (@Title, @AuthorName, @AuthorEmail, @AuthorGender, @Price)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Title", book.Title);
                            command.Parameters.AddWithValue("@AuthorName", book.Author.FullName);
                            command.Parameters.AddWithValue("@AuthorEmail", book.Author.Email);
                            command.Parameters.AddWithValue("@AuthorGender", book.Author.Gender);
                            command.Parameters.AddWithValue("@Price", book.Price);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving books to database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBooksFromDb()
        {
            List<Book> books = new List<Book>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT Title, AuthorName, AuthorEmail, AuthorGender, Price FROM Books";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string title = reader["Title"].ToString();
                                string authorName = reader["AuthorName"].ToString();
                                string authorEmail = reader["AuthorEmail"].ToString();
                                char authorGender = Convert.ToChar(reader["AuthorGender"]);
                                double price = Convert.ToDouble(reader["Price"]);

                                Author author = new Author(authorName, authorEmail, authorGender);
                                Book book = new Book(title, author, price);
                                books.Add(book);
                            }
                        }
                    }
                }
                library.LoadBooks(books);
                BooksListBox.ItemsSource = library.GetAllBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books from database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAddBookClicked(object sender, RoutedEventArgs e)
        {
            var title = PromptDialog("Add Book", "Enter the book title:");
            var authorName = PromptDialog("Add Book", "Enter the author's full name:");
            var authorEmail = PromptDialog("Add Book", "Enter the author's email:");
            var authorGenderStr = PromptDialog("Add Book", "Enter the author's gender (M/F):");
            var priceStr = PromptDialog("Add Book", "Enter the book price:");

            if (double.TryParse(priceStr, out double price) && char.TryParse(authorGenderStr, out char authorGender))
            {
                Author author = new Author(authorName, authorEmail, authorGender);
                Book book = new Book(title, author, price);
                library.AddBook(book);
                MessageBox.Show("Book added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BooksListBox.ItemsSource = library.GetAllBooks();
            }
            else
            {
                MessageBox.Show("Invalid input. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSearchBookClicked(object sender, RoutedEventArgs e)
        {
            var title = PromptDialog("Search Book", "Enter the book title to search:");
            Book book = library.SearchBookByTitle(title);

            if (book != null)
            {
                MessageTextBlock.Text = $"Found book: {book}";
            }
            else
            {
                MessageTextBlock.Text = "Book not found.";
            }
        }

        private void OnRemoveBookClicked(object sender, RoutedEventArgs e)
        {
            var title = PromptDialog("Remove Book", "Enter the book title to remove:");
            bool isRemoved = library.RemoveBook(title);

            if (isRemoved)
            {
                MessageBox.Show("Book removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BooksListBox.ItemsSource = library.GetAllBooks();
            }
            else
            {
                MessageBox.Show("Book not found.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnListAllBooksClicked(object sender, RoutedEventArgs e)
        {
            BooksListBox.ItemsSource = library.GetAllBooks();
        }

        private void OnCheckWordClicked(object sender, RoutedEventArgs e)
        {
            var word = PromptDialog("Check Word", "Enter the word to check in book titles:");
            bool containsWord = library.ContainsTitle(word);

            if (containsWord)
            {
                MessageBox.Show($"Library contains a book with the word '{word}' in the title.", "Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No books found with the word '{word}' in the title.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            SaveBooksToDb();
            Application.Current.Shutdown();
        }

        private string PromptDialog(string title, string message)
        {
            var dialog = new InputDialog(title, message);
            if (dialog.ShowDialog() == true)
            {
                return dialog.ResponseText;
            }
            return string.Empty;
        }
        private void OnAddReaderClicked(object sender, RoutedEventArgs e)
        {
            var readerName = PromptDialog("Add Reader", "Enter the reader's name:");
            if (!string.IsNullOrEmpty(readerName))
            {
                Reader reader = new Reader(readerName);
                library.AddReader(reader);
                MessageBox.Show("Reader added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Invalid input. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnListAllReadersClicked(object sender, RoutedEventArgs e)
        {
            var allReaders = library.GetAllReaders();
            MessageBox.Show(string.Join(Environment.NewLine, allReaders), "All Readers", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnAddFavoriteBookToReaderClicked(object sender, RoutedEventArgs e)
        {
            var readerName = PromptDialog("Add Favorite Book to Reader", "Enter the reader's name:");
            var bookTitle = PromptDialog("Add Favorite Book to Reader", "Enter the book title:");

            var reader = library.SearchReaderByName(readerName);
            var book = library.SearchBookByTitle(bookTitle);

            if (reader != null && book != null)
            {
                reader.AddFavoriteBook(book);
                MessageBox.Show("Favorite book added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Reader or Book not found. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Library
    {
        private List<Book> books;
        private List<Reader> readers;

        public Library()
        {
            books = new List<Book>();
            readers = new List<Reader>();
        }

        public void AddBook(Book book)
        {
            books.Add(book);
        }

        public Book SearchBookByTitle(string title)
        {
            return books.Find(book => book.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveBook(string title)
        {
            var book = SearchBookByTitle(title);
            if (book != null)
            {
                books.Remove(book);
                return true;
            }
            return false;
        }

        public List<Book> GetAllBooks()
        {
            return new List<Book>(books);
        }

        public bool ContainsTitle(string word)
        {
            return books.Exists(book => book.Title.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        public void LoadBooks(List<Book> loadedBooks)
        {
            books = loadedBooks ?? new List<Book>();
        }

        public void AddReader(Reader reader)
        {
            readers.Add(reader);
        }

        public List<Reader> GetAllReaders()
        {
            return new List<Reader>(readers);
        }

        public Reader SearchReaderByName(string name)
        {
            return readers.Find(reader => reader.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveReader(string name)
        {
            var reader = SearchReaderByName(name);
            if (reader != null)
            {
                readers.Remove(reader);
                return true;
            }
            return false;
        }
    }
}
