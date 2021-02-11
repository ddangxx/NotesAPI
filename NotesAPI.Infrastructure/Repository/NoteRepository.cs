using NotesAPI.Infrastructure.Configuration;
using NotesAPI.Infrastructure.Entities;
using NotesAPI.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NotesAPI.Infrastructure.Interface
{
    public class NoteRepository : INoteRepository
    {
        private static Notes noteContainer = null;

        private NotesConfiguration _configuration;
        private string jsonFile;
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        public NoteRepository(NotesConfiguration configuration)
        {
            _configuration = configuration;
            jsonFile = $"{_configuration.FilePath}";
        }

        public Notes MapNote()
        {
            Notes notes = null;
            string json = string.Empty;

            // static noteCotainer acts as a cache
            // so we do not read directly from the file for every request
            if (noteContainer == null)
            {
                try
                {
                    json = System.IO.File.ReadAllText(jsonFile);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    /* handling exception if there is no json file
                     * get list will return error File is Empty
                     * create note will create a json file
                     */
                }


                if (!string.IsNullOrEmpty(json))
                {
                    noteContainer = Newtonsoft.Json.JsonConvert.DeserializeObject<Notes>(json);
                    notes = noteContainer;
                }
                else
                {
                    noteContainer = null;
                }
            }
            else
            {
                notes = noteContainer;
            }

            return notes;

        }

        public void SaveNote(Notes notes)
        {
            // cacheLock is used for thread safe
            // multiple reads but only 1 write at a time
            try
            {
                cacheLock.EnterWriteLock();
                System.IO.File.WriteAllText(jsonFile, Newtonsoft.Json.JsonConvert.SerializeObject(notes));
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
    }
}
