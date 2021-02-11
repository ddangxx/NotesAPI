using NotesAPI.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NotesAPI.Infrastructure.Interfaces
{
    public interface INoteRepository
    {
        Notes MapNote();
        void SaveNote(Notes notes);
    }
}
