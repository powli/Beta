using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Beta.Repository
{
    [Serializable]
    public class NoteRepository
    {
        [XmlArrayItem("Notes")]
        public List<Note> Notes { get; set; }

        public NoteRepository()
        {
            Notes = new List<Note>();
        }

        public Note GetNote(string name)
        {
            return Notes.FirstOrDefault(n => n.Name == name);
        }

        public Note AddNote(string noteName, string text, string addedBy)
        {
            string name = noteName.Trim();
            Note note = new Note
            {
                Name = name,
                Text = text,
                AddedBy = addedBy,
                DateAdded = DateTime.Now
            };

            Notes.Add(note);
            return note;
        }

        public string DeleteQuote(string name)
        {
            Note temp = Notes.FirstOrDefault(n => n.Name == name);
            string response = "Sorry, I was unable to find a note with the name '"+name+"'.";
            if (temp != null)
            {                
                response = "Successfully removed the following note: '" + GetNote(name).Text + "'.";
                Notes.Remove(temp);
                return response;
            }
            return response;

        }

        internal bool NoteExists(string name)
        {
            return Notes.FirstOrDefault(n => n.Name == name) != null;
        }
    }

    public class Note
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Text { get; set; }
        [XmlAttribute]
        public string AddedBy { get; set; }
        [XmlAttribute]
        public DateTime DateAdded { get; set; }
    }
}
