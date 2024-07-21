using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;

namespace Library
{
    internal class Program
    {
        public class Author
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public char Gender { get; set; }

            public Author(string fullName, string email, char gender)
            {
                FullName = fullName;
                Email = email;
                Gender = gender;
            }
            public override string ToString()
            {
                return $"Author[name={FullName},email={Email},gender={Gender}]";
            }
        }

        public class Book
        {
            public string Title { get; set; }
            public Author Author { get; set; }
            public double Price { get; set; }

            public Book(string title, Author author, double price)
            {
                Title = title;
                Author = author;
                Price = price;
            }
            public override string ToString()
            {
                return $"Book[name={Title},Author[{Author}],price={Price}]";
            }
        }

        public class Reader
        {
            public string Name { get; set; }
            public List<Book> FavoriteBooks { get; set; }

            public Reader(string name)
            {
                Name = name;
                FavoriteBooks = new List<Book>();
            }

            public void AddFavoriteBook(Book book)
            {
                if (!FavoriteBooks.Contains(book))
                {
                    FavoriteBooks.Add(book);
                }
            }

            public override string ToString()
            {
                var favoriteBooksTitles = FavoriteBooks.Select(book => book.Title).ToList();
                return $"Reader[name={Name}, favoriteBooks={string.Join(", ", favoriteBooksTitles)}]";
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

            // Book management methods
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

            // Reader management methods
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
}
