using Throb.Data.DbContext;
using Throb.Data.Entities;
using System.Linq;

using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class AssignmentRepository : GenericRepository<Assignment>,IAssignmentRepository
    {
        private readonly ThrobDbContext _context;

        public AssignmentRepository(ThrobDbContext context) : base(context)
        {
            _context = context;
        }

        //public void Add(Assignment assigment)
        // =>_context.Add(assigment);

        //public void Delete(Assignment assigment)
        // =>_context.Remove(assigment);

        //public IEnumerable<Assignment> GetAll() => _context.Assignments.ToList();

        //public Assignment GetById(int id)

        //=>_context.Assignments.Find(id);

        //public void Update(Assignment assigment)
        //    =>_context.Update(assigment);

    }
}
