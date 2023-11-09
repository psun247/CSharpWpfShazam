using System.Collections.Generic;

// Multiple classes in this file
namespace ClientServerShared
{    
    public class GetAllSongInfoListRequest
    {
    }

    public class GetAllSongInfoListResponse
    {
        public List<SongInfoDTO> SongInfoDtoList { get; set; } = new List<SongInfoDTO>();
    }

    public class AddSongInfoRequest
    {
        public SongInfoDTO SongInfoDTO { get; set; } = new SongInfoDTO();
    }

    public class AddSongInfoResponse
    {
        public string Error { get; set; } = string.Empty;
    }

    public class DeleteSongInfoRequest
    {
        public string SongUrl { get; set; } = string.Empty;
    }

    public class DeleteSongInfoResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}
