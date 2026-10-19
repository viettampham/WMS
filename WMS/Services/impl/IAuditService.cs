using WMS.Models;
using WMS.Models.Request.Audit;
using WMS.Models.Response.Audit;
using WMS.Models.Response.Common;
using WMS.Models.Response.Cont;

namespace WMS.Services.impl
{
    public interface IAuditService
    {
        Task<CommonResponseModel<PagingResponse<AuditResponse>>> GetAudit(GetAuditRequest req);
        Task<CommonResponseModel<string>> CreateAudit(CreateAuditRequest req);
        Task<CommonResponseModel<string>> DeleteAudit(int Id);

        Task<CommonResponseModel<OperationStandard>> GetOperation();
        Task<CommonResponseModel<DetechIssue>> GetDetechByOps(int id);

        Task<CommonResponseModel<string>> GetAssignee();
        Task<CommonResponseModel<string>> CreateAuditDetail(CreateAuditDetailRequest req);
        Task<CommonResponseModel<string>> CreateAuditDetailDaily(CreateAuditDetailRequest req);
        Task<CommonResponseModel<PagingResponse<AuditDetailResponse>>> GetAuditDetail(GetAuditDetailRequest req);

        Task<CommonResponseModel<string>> GetLocation();

        Task<CommonResponseModel<AuditDetailResponse>> GetAuditDetailByID(int id);

        Task<CommonResponseModel<string>> AddImageAfterToAuditDetail(AddImageToAuditDetail req);
    }
}
