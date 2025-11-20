using System.Collections.Generic;
using Shopify.Models;

namespace Shopify.ViewModels
{
    public class HomeViewModel
    {
        public List<Song> RecentSongs { get; set; }
        public List<Song> TrendingSongs { get; set; }
        public List<Artist> PopularArtists { get; set; }
        public List<Album> PopularAlbums { get; set; }
    }
}
