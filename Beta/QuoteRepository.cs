using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Beta
{
    [Serializable]
    public class QuoteRepository
    {
        private const string _Filename = "Quotes.xml";

        [XmlArrayItem("Author")]
        public List<Author> Authors
        {
            get;
            set;
        }

        public QuoteRepository()
        {
            Authors = new List<Author>();
        }

        /// <summary>
        /// Get quotes for author (will return null if author name not found)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Author GetAuthor(string name)
        {
            name = name.Trim();
            return Authors.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Add a quote by the specified author name
        /// </summary>
        public Quote AddQuote(string authorName, string text, string addedBy)
        {
            authorName = authorName.Trim();
            Author existingAuthor = Authors.FirstOrDefault(a => string.Equals(a.Name, authorName, StringComparison.CurrentCultureIgnoreCase));

            Quote quote = new Quote
            {
                Text = text,
                DateAdded = DateTime.Now,
                AddedBy = addedBy,
            };

            if (existingAuthor != null)
            {
                existingAuthor.Quotes.Add(quote);
            }
            else
            {
                Authors.Add(
                    new Author(authorName)
                    {
                        DateAdded = DateTime.Now,
                        AddedBy = addedBy,
                        Quotes = { quote },
                    });
            }

            return quote;
        }

        /// <summary>
        /// Save to file
        /// </summary>
        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QuoteRepository));
            using (FileStream file = File.Create(_Filename))
            {
                serializer.Serialize(file, this);
            }
        }

        /// <summary>
        /// Load from disk
        /// </summary>
        /// <returns></returns>
        public static QuoteRepository LoadFromDisk()
        {
            QuoteRepository dictionary;

            if (File.Exists(_Filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(QuoteRepository));
                using (FileStream file = new FileStream(_Filename, FileMode.Open))
                {
                    dictionary = serializer.Deserialize(file) as QuoteRepository;
                }
            }
            else
            {
                dictionary = new QuoteRepository();
                dictionary.Save();
            }

            return dictionary;
        }
    }

    [Serializable]
    public class Author
    {
        [XmlAttribute]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute]
        public string AddedBy
        {
            get;
            set;
        }

        [XmlAttribute]
        public DateTime DateAdded
        {
            get;
            set;
        }

        [XmlArrayItem("Quote")]
        public List<Quote> Quotes
        {
            get;
            set;
        }

        public Author(string name)
        {
            Quotes = new List<Quote>();
            Name = name;
        }

        public Author()
            : this(string.Empty)
        {
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} Quote(s)", Name, Quotes.Count);
        }
    }

    [Serializable]
    public class Quote
    {
        [XmlAttribute]
        public string Text
        {
            get;
            set;
        }

        [XmlAttribute]
        public string AddedBy
        {
            get;
            set;
        }

        [XmlAttribute]
        public DateTime DateAdded
        {
            get;
            set;
        }
    }
}
