using Throb.Data.DbContext;
using Throb.Data.Entities;
using System.Linq;

using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class PdfRepository : GenericRepository<Pdf>, IPdfRepository
    {
        private readonly ThrobDbContext _context;

        public PdfRepository(ThrobDbContext context) : base(context)
        {
            _context = context;
        }

        //public void Add(Pdf pdf)
        // =>_context.Add(pdf);

        //public void Delete(Pdf pdf)
        // =>_context.Remove(pdf);

        //public IEnumerable<Pdf> GetAll() => _context.Pdfs.ToList();

        //public Pdf GetById(int id)

        //=>_context.Pdfs.Find(id);

        //public void Update(Pdf pdf)
        //    =>_context.Update(pdf);

    }
}
