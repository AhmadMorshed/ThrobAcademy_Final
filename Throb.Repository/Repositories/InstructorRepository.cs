using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Throb.Data.DbContext;
using Throb.Data.Entities;
using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class InstructorRepository :GenericRepository<Instructor>, IInstructorRepository
    {
        private readonly ThrobDbContext _context;

        public InstructorRepository(ThrobDbContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<Instructor> GetInstructorByName(string name)
                => _context.Instructors.Where(s => s.Name.Trim().ToLower().Contains(name.Trim().ToLower())).ToList();


        IQueryable<Instructor> IInstructorRepository.GetAll()
        {
            return _context.Instructors
                .Include(s => s.Courses);
        }

        //public void Add(Instructor instructor)
        //    =>_context.Add(instructor);


        //public void Delete(Instructor instructor)
        //   =>_context.Remove(instructor);

        //public IEnumerable<Instructor> GetAll()
        // =>_context.Instructors.ToList();

        //public Instructor GetById(int id)
        //=> _context.Instructors.Find();

        //public void Update(Instructor instructor)
        //=>_context.Update(instructor);
    }
}
