using System;
using System.Collections.Generic;

namespace NotesAPI.Infrastructure.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class Notes
    {
        public List<Note> NotesList { get; set; }
        public Notes()
        {
            this.NotesList = new List<Note>();
        }
    }
}
