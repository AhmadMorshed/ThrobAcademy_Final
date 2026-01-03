using Throb.Data.DbContext;
using Throb.Data.Entities;
using System.Linq;

using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class VideoRepository : GenericRepository<Video>, IVideoRepository
    {
        private readonly ThrobDbContext _context;

        public VideoRepository(ThrobDbContext context):base(context) 
        {
            _context = context;
        }

        //public void Add(Video video)
        // =>_context.Add(video);

        //public void Delete(Video video)
        // =>_context.Remove(video);

        //public IEnumerable<Video> GetAll() => _context.Videos.ToList();

        //public Video GetById(int id)

        //=>_context.Videos.Find(id);

        //public void Update(Video video)
        //    =>_context.Update(video);

    }
}
