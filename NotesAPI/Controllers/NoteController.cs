using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NotesAPI.Infrastructure.Configuration;
using NotesAPI.Infrastructure.Entities;
using System.Linq;
using System.IO;
using System.Threading;

namespace NotesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private static Notes noteContainer = null;

        private NotesConfiguration _configuration;
        private string jsonFile;
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        public NoteController(NotesConfiguration configuration)
        {
            _configuration = configuration;
            jsonFile = $"{_configuration.FilePath}";
        }

        [HttpGet]
        //[Route("api/note")]
        public IActionResult GetNotes()
        {

            Notes notes = MapNote();
            if (notes != null)
            {
                return Ok(notes);
            }
            else
            {
                return NotFound("File Is Empty");
            }
        }

        //[Route("api/note/{id}")]
        [HttpGet("{id}")]
        public IActionResult GetNote(int id)
        {            

            Notes notes = MapNote();
            if (notes != null)
            {
                
                var result = notes.NotesList.Find(n => n.Id == id);

                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("ID Not Found");
                }
            }
            else
            {
                return NotFound("File Is Empty");
            }
        }

        //[Route("api/note/")]
        [HttpPost]
        public IActionResult CreateNote([FromBody] string newNote)
        {
            try
            {
                int id;
                Notes notes = MapNote();
                if(notes != null)
                {
                    // in case id is not in ascending order
                    id = notes.NotesList.Max(n => n.Id) + 1;                    
                }
                else
                {
                    id = 1;
                    notes = new Notes();
                }

                var note = new Note()
                {
                    Id = id,
                    Data = newNote,
                    DateCreated = DateTime.Now
                };

                notes.NotesList.Add(note);
                                
                SaveNote(notes);

                return StatusCode(201);
            }
            catch (IOException ex)
            { 
                return StatusCode(424);
            }
        }

        //[Route("api/note/{id}")]
        [HttpDelete("{id}")]
        public IActionResult DeleteNote(int id)
        {

            Notes notes = MapNote();
            if (notes != null)
            {

                var noteToRemove = notes.NotesList.Find(n => n.Id == id);

                if (noteToRemove != null)
                {
                    bool response = notes.NotesList.Remove(noteToRemove);

                    if (response)
                    {                        
                        SaveNote(notes);
                        return Ok("Successfully Deleted Note");
                    }
                    else
                    {
                        return StatusCode(500);
                    }
                }
                else
                {
                    return BadRequest("ID Not Found");
                }
            }
            else
            {
                return NotFound("File Is Empty");
            }
        }

        //[Route("api/note/{id}")]
        [HttpPut("{id}")]
        public IActionResult UpdateNote([FromRoute] int id, [FromBody] string newNote)
        {
            Notes notes = MapNote();
            if (notes != null)
            {

                int index = notes.NotesList.FindIndex(n => n.Id == id);

                if (index >= 0)
                {
                    notes.NotesList[index].Data = newNote;
                    notes.NotesList[index].DateCreated = DateTime.Now;                    
                    SaveNote(notes);
                    return Ok("Successfully Updated Note");
                }
                else
                {
                    return BadRequest("ID Not Found");
                }
            }
            else
            {
                return NotFound("File Is Empty");
            }

        }

        private Notes MapNote()
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

        private void SaveNote(Notes notes)
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
