using WMS.Models.Request.Cont;
using WMS.Models.Response.Common;
using WMS.Models.Response.Cont;

namespace WMS.Services.impl
{
    public interface IDeclareContService
    {
        Task<CommonResponseModel<PagingResponse<DeclareContResponse>>> GetList(GetDeclateContRequest req);
        Task<CommonResponseModel<ConImageResponseWithIDCont>> Declare(DeclareContRequest req);

        Task<CommonResponseModel<ConImageResponse>> AddImageToCont(AddImageToContRequest req);
        Task<(byte[] FileBytes, string FileName)> DownloadFolder(int id);
    }
}
