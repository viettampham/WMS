using AutoMapper;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.AccessControl;
using WMS.Models;
using WMS.Models.Request;
using WMS.Models.Request.Audit;
using WMS.Models.Response.Audit;
using WMS.Models.Response.Common;
using WMS.Services.impl;


namespace WMS.Services.Service
{
    public class AuditService : IAuditService
    {
        private readonly LienVietStorageContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly EmailSetting _emailSettings;
        public AuditService(LienVietStorageContext context, ICurrentUserService currentUserService, IMapper mapper, IOptions<EmailSetting> emailSettings) { 
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _emailSettings = emailSettings.Value;
        }

        public async Task<CommonResponseModel<string>> CreateAudit(CreateAuditRequest req)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try
            {
                Audit newi = new Audit();
                newi.Auditor = req.Auditor;
                newi.Location = req.Location;
                newi.DateCreate = DateTime.Now;
                newi.Note = req.Note;
                newi.Status = "Mới tạo";

                await _context.Audits.AddAsync(newi);
                await _context.SaveChangesAsync();
                res.Message = "Thêm mới thành công";
                res.Status = "SUCCESS";
                return res;
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> CreateAuditDetail(CreateAuditDetailRequest req)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try {
                bool exists = await _context.AuditDetails
                        .AnyAsync(x => x.AuditId == req.IdAudit);
                if (!exists)
                {
                    List<DetechIssue> lstDetechIssues = await _context.DetechIssues.ToListAsync();
                    List<AuditDetail> lstAdd = new List<AuditDetail>();

                    var operationStandards = await _context.OperationStandards
                        .ToDictionaryAsync(
                            x => x.Id
                        );

                    foreach (var item in lstDetechIssues)
                    {
                        AuditDetail newi = new AuditDetail();
                        newi.AuditId = req.IdAudit;
                        operationStandards.TryGetValue(
                            item.IdOperationStandard,
                            out var operationStandard);
                        newi.OperationStandard = operationStandard?.OperationStandard1;
                        newi.DetechIssue = item.DetechIssue1;
                        if (item.Id == req.IdDetachIssue)
                        {
                            newi.Assignee = req.Assignee;
                            if (DateTime.TryParse(req.DueDate, out var dueDate))
                            {
                                newi.DueDate = dueDate;
                            }
                            newi.Status = "Mới phát hiện";
                            newi.Note = req.Note;
                            newi.Evalue = req.Evalue;
                            newi.ImageAfter = "";
                        }

                        lstAdd.Add(newi);
                    }
                    await _context.AuditDetails.AddRangeAsync(lstAdd);
                    await _context.SaveChangesAsync();

                    res.Message = "Thành công";
                    res.Status = "SUCCESS";
                    return res;
                }
                else {
                    DetechIssue tgrDetechIssue = await _context.DetechIssues.Where(x => x.Id == req.IdDetachIssue).FirstOrDefaultAsync();
                    AuditDetail tgr = await _context.AuditDetails.Where(x => x.AuditId == req.IdAudit && x.DetechIssue == tgrDetechIssue.DetechIssue1).FirstOrDefaultAsync();
                    if (tgr != null)
                    {
                        tgr.Assignee = req.Assignee;
                        if (DateTime.TryParse(req.DueDate, out var dueDate))
                        {
                            tgr.DueDate = dueDate;
                        }
                        tgr.Status = "Mới phát hiện";
                        tgr.Note = req.Note;
                        tgr.Evalue = req.Evalue;
                        tgr.ImageAfter = "";

                        await _context.SaveChangesAsync();

                        res.Message = "Thành công";
                        res.Status = "SUCCESS";
                        return res;
                    }
                    else {
                        AuditDetail newi = new AuditDetail();
                        newi.AuditId = req.IdAudit;
                        var operationStandard = await _context.OperationStandards.FirstOrDefaultAsync(x => x.Id == tgrDetechIssue.IdOperationStandard);
                        newi.OperationStandard = operationStandard?.OperationStandard1;
                        newi.DetechIssue = tgrDetechIssue.DetechIssue1;
                        newi.Assignee = req.Assignee;
                        if (DateTime.TryParse(req.DueDate, out var dueDate))
                        {
                            newi.DueDate = dueDate;
                        }
                        newi.Status = "Mới phát hiện";
                        newi.Note = req.Note;
                        newi.Evalue = req.Evalue;
                        newi.ImageAfter = "";
                        await _context.AuditDetails.AddAsync(newi);
                        await _context.SaveChangesAsync();
                        res.Message = "Thành công";
                        res.Status = "SUCCESS";
                        return res;
                    }
                }
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> CreateAuditDetailDaily(CreateAuditDetailRequest req)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try
            {
                PathConfig pathConfig = await _context.PathConfigs.Where(x => x.PathName == "OperationDetechImage").FirstOrDefaultAsync();
                if (pathConfig == null)
                {
                    res.Message = "Chưa cấu hình pathconfig cho 'OperationDetech'";
                    res.Status = "WARNING";
                    return res;
                }

                string folderPath = pathConfig.PathLocation;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string extension = Path.GetExtension(req.ImageBefore.FileName);
                string fileName = $"{req.IdOps}_{req.IdDetachIssue}_{Guid.NewGuid()}{extension}.png";

                string filePath = Path.Combine(folderPath, fileName);

                string directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
               
                await CompressImageTo200KbAsync(req.ImageBefore, filePath);

                AuditDetail newi = new AuditDetail();
                newi.AuditId = req.IdAudit;
                newi.OperationStandard = await _context.OperationStandards
                    .Where(x => x.Id == req.IdOps)
                    .Select(x => x.OperationStandard1)
                    .FirstOrDefaultAsync();
                newi.DetechIssue = await _context.DetechIssues.Where(x=>x.Id == req.IdDetachIssue).Select(x=>x.DetechIssue1).FirstOrDefaultAsync();
                newi.Assignee = req.Assignee;
                if (!String.IsNullOrEmpty(req.DueDate))
                {
                    newi.DueDate = Convert.ToDateTime(req.DueDate);
                }
                newi.Status = req.Status;
                newi.IsPrivate = req.IsPrivate;
                newi.Note = req.Note;
                newi.Evalue = req.Evalue;
                newi.Location = req.Location;
                newi.DateCreate = DateTime.Now;
                newi.ImageBefore = fileName;
                newi.Owner = req.Owner;
                await _context.AuditDetails.AddAsync(newi);
                await _context.SaveChangesAsync();


                //gửi mail khi phát hành
                if (req.Status == "Phát hành")
                {
                    string body = $@"
                        <div style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>

                            <div style='background:#1F4E79;color:#fff;padding:15px 20px;border-radius:6px 6px 0 0;'>
                                <h2 style='margin:0;'>THÔNG BÁO PHÂN CÔNG AUDIT TASK</h2>
                            </div>

                            <div style='border:1px solid #d9d9d9;padding:20px;border-radius:0 0 6px 6px;'>

                                <p>Kính gửi <b>{newi.Assignee}</b>,</p>

                                <p>
                                    Hệ thống vừa ghi nhận một nhiệm vụ Audit mới được phân công đến Quý Anh/Chị.
                                    Vui lòng xem thông tin chi tiết bên dưới và thực hiện xử lý theo đúng thời hạn.
                                </p>

                                <table style='border-collapse:collapse;width:100%;margin-top:15px;'>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;width:180px;'>
                                            Nội dung vấn đề
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {newi.Note}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;width:180px;'>
                                            Vi phạm mục
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {newi.OperationStandard} - {newi.DetechIssue}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Địa điểm
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {newi.Location}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Người phát hiện
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {newi.Owner} - phát hiện ngày {newi.DateCreate:dd/MM/yyyy}
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Người phụ trách
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {newi.Assignee}
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Hạn hoàn thành
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;color:#C00000;font-weight:bold;'>
                                            {newi.DueDate:dd/MM/yyyy}
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Trạng thái
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;color:#C00000;font-weight:bold;'>
                                            {newi.Status}
                                        </td>
                                    </tr>

                                </table>

                                <p style='margin-top:20px;'>
                                    Đề nghị Quý Anh/Chị đăng nhập vào hệ thống Audit để cập nhật nguyên nhân,
                                    hành động khắc phục và tiến độ thực hiện.
                                </p>

                                <p>
                                    Trường hợp đã hoàn thành xử lý, vui lòng cập nhật trạng thái trên hệ thống để phục vụ công tác theo dõi và đánh giá.
                                </p>

                                <hr style='margin:25px 0;border:none;border-top:1px solid #ddd;'>

                                <p style='font-size:12px;color:#666;'>
                                    Đây là thư thông báo được gửi tự động từ hệ thống Audit.<br/>
                                    Vui lòng không trả lời thư điện tử này.
                                </p>

                            </div>
                        </div>";

                    User userTo = await _context.Users.Where(x => x.FullName == newi.Assignee).FirstOrDefaultAsync();
                    User userCC = await _context.Users.Where(x => x.FullName == newi.Owner).FirstOrDefaultAsync();

                    await SendMail(
                        userTo.Email,
                        userCC.Email,
                        "Audit Task Assignment",
                        body);
                }

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                return res;
            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task CompressImageTo200KbAsync(IFormFile file, string outputPath)
        {
            using var stream = file.OpenReadStream();
            using var image = new MagickImage(stream);

            if (image.Width > 1280)
            {
                image.Resize(1280U, 0);
            }

            const int targetSize = 200 * 1024;

            for (int quality = 90; quality >= 20; quality -= 5)
            {
                image.Quality = (uint)quality;

                using var ms = new MemoryStream();
                image.Write(ms, MagickFormat.Jpeg);

                if (ms.Length <= targetSize)
                {
                    await File.WriteAllBytesAsync(outputPath, ms.ToArray());
                    return;
                }
            }

            image.Quality = 20;
            image.Write(outputPath, MagickFormat.Jpeg);
        }

        public async Task<CommonResponseModel<string>> DeleteAudit(int Id)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try
            {
                Audit iad = await _context.Audits.Where(x => x.Id == Id).FirstOrDefaultAsync();
                if (iad == null)
                {
                    res.Message = "Không tồn tại audit này / đã bị xoá từ trước";
                    res.Status = "ERROR";
                }
                else { 
                    _context.Audits.Remove(iad);
                    await _context.SaveChangesAsync();
                    res.Message = "Xoá thành công audit";
                    res.Status = "SUCCESS";
                }
                return res;
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> GetAssignee()
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try
            {
                List<string> lst = await _context.Users.Select(x => x.FullName).ToListAsync();
                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.ListData = lst;
                return res;
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<PagingResponse<AuditResponse>>> GetAudit(GetAuditRequest req)
        {
            CommonResponseModel<PagingResponse<AuditResponse>> res = new CommonResponseModel<PagingResponse<AuditResponse>>();
            //PagingResponse<AuditResponse> pres = new PagingResponse<AuditResponse>();
            try
            {
                var fromDate = req.DateCreateFrom.Date; // 00:00:00

                var toDate = req.DateCreateTo.Date.AddDays(1).AddTicks(-1);
                // 23:59:59.9999999

                var query = _context.Audits.Where(x =>
                    (x.Auditor.Contains(req.Auditor) || req.Auditor == "") &&
                    (x.Location.Contains(req.Location) || req.Location == "") &&
                    x.DateCreate >= fromDate &&
                    x.DateCreate <= toDate
                );

                var totalRecords = query.Count();

                var audits = await query
                .Skip((req.PageIndex - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var data = _mapper.Map<List<AuditResponse>>(audits);

                var paging = new PagingResponse<AuditResponse>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = req.PageIndex,
                    PageSize = req.PageSize
                };

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.Data = paging;

                return res;
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<PagingResponse<AuditDetailResponse>>> GetAuditDetail(GetAuditDetailRequest req)
        {
            CommonResponseModel<PagingResponse<AuditDetailResponse>> res = new CommonResponseModel<PagingResponse<AuditDetailResponse>>();
            try {
                var query = _context.AuditDetails.Where(x => (x.Assignee.Contains(req.Assignee) || req.Assignee == "")
                && (x.Status == req.Status || req.Status == "")).OrderByDescending(x=>x.Id).AsQueryable();

                var totalRecords = query.Count();

                var users = await query
                .Skip((req.PageIndex - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var data = _mapper.Map<List<AuditDetailResponse>>(users);

                var paging = new PagingResponse<AuditDetailResponse>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = req.PageIndex,
                    PageSize = req.PageSize
                };

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.Data = paging;

                return res;

            }
            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<DetechIssue>> GetDetechByOps(int id)
        {
            CommonResponseModel<DetechIssue> res = new CommonResponseModel<DetechIssue>();
            try {
                List<DetechIssue> lst = await _context.DetechIssues.Where(x => x.IdOperationStandard == id).ToListAsync();
                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.ListData = lst;

                return res;
            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<OperationStandard>> GetOperation()
        {
            CommonResponseModel<OperationStandard> res = new CommonResponseModel<OperationStandard>();
            try {
                List<OperationStandard> lst = await _context.OperationStandards.ToListAsync();
                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.ListData = lst;

                return res;
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> GetLocation()
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try {
                List<string> lst = await _context.Locations.Select(x => x.Location1).ToListAsync();
                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.ListData = lst;
                return res;
            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<AuditDetailResponse>> GetAuditDetailByID(int id)
        {
            CommonResponseModel<AuditDetailResponse> res = new CommonResponseModel<AuditDetailResponse>();
            try
            {

                AuditDetailResponse result = await _context.AuditDetails
                    .Where(x => x.Id == id)
                    .Select(x => new AuditDetailResponse
                    {
                        Id = x.Id,
                        OperationStandard = x.OperationStandard,
                        DetechIssue = x.DetechIssue,
                        Assignee = x.Assignee,
                        DueDate = x.DueDate,
                        ImageBefore = x.ImageBefore,
                        ImageAfter = x.ImageAfter,
                        Status = x.Status,
                        Note = x.Note,
                        Evalue = x.Evalue,
                        DoneTime = x.DoneTime,
                        Location = x.Location,
                        DateCreate = x.DateCreate,
                        Owner = x.Owner,
                        ActionPlan = x.PlanAction
                    })
                    .FirstOrDefaultAsync();

                string path = await _context.PathConfigs.Where(x => x.PathName == "OperationDetechImage").Select(x => x.PathLocation).FirstOrDefaultAsync();

                if (result.ImageBefore != null) {
                    string pathImage = Path.Combine(path, result.ImageBefore);
                    byte[] bytes = await System.IO.File.ReadAllBytesAsync(pathImage);
                    string base64 = Convert.ToBase64String(bytes);
                    result.ImageBefore = $"data:image/png;base64,{base64}";
                }
                if (result.ImageAfter != null)
                {
                    string pathImage = Path.Combine(path, result.ImageAfter);
                    byte[] bytes = await System.IO.File.ReadAllBytesAsync(pathImage);
                    string base64 = Convert.ToBase64String(bytes);
                    result.ImageAfter = $"data:image/png;base64,{base64}";
                }

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.Data = result;

                return res;
            }
            catch (Exception ex) {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> AddImageAfterToAuditDetail(AddImageToAuditDetail req)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try {
                AuditDetail tar = await _context.AuditDetails.Where(x => x.Id == req.IdAuditDetail).FirstOrDefaultAsync();
                if (tar == null)
                {
                    res.Status = "Không tìm thấy thông tin chi tiết audit";
                    res.Status = "ERROR";
                    return res;
                }
                else {
                    PathConfig pathConfig = await _context.PathConfigs.Where(x => x.PathName == "OperationDetechImage").FirstOrDefaultAsync();
                    if (pathConfig == null)
                    {
                        res.Message = "Chưa cấu hình pathconfig cho 'OperationDetech'";
                        res.Status = "WARNING";
                        return res;
                    }

                    string folderPath = pathConfig.PathLocation;
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string extension = Path.GetExtension(req.ImageAfter.FileName);
                    string fileName = $"{req.IdAuditDetail}_{Guid.NewGuid()}{extension}.png";

                    //string pathSave = Path.Combine(checkCont.SoBooking, checkCont.SoCont, fileName);
                    string filePath = Path.Combine(folderPath, fileName);

                    string directoryPath = Path.GetDirectoryName(filePath);

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    await CompressImageTo200KbAsync(req.ImageAfter, filePath);
                    //string imagePath = Path.Combine("ContImages", fileName);

                    tar.ImageAfter = fileName;
                    tar.DoneTime = DateTime.Now;
                    tar.Status = "Đã hoàn thành";
                    tar.PlanAction = req.ActionPlan;
                    await _context.SaveChangesAsync();

                    string subject = $"[AUDIT SYSTEM] Công việc Audit đã hoàn thành";

                    User userTo = await _context.Users.Where(x => x.FullName == tar.Owner).FirstOrDefaultAsync();
                    User userCC = await _context.Users.Where(x => x.FullName == tar.Assignee).FirstOrDefaultAsync();

                    string body = $@"
                    <div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#333;'>

                        <div style='background-color:#1F4E78;padding:15px;color:white;'>
                            <h2 style='margin:0;'>✅ AUDIT TASK COMPLETED</h2>
                        </div>

                        <div style='padding:20px;border:1px solid #d9d9d9;border-top:none;'>

                            <p>Xin chào <b>{userTo.FullName}</b>,</p>

                            <p>
                                Hệ thống ghi nhận công việc Audit đã được xử lý và hoàn thành.
                                Thông tin chi tiết như sau:
                            </p>

                            <table style='border-collapse:collapse;width:100%;margin-top:15px;'>
                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;width:220px;'>
                                        <b>Người phát hiện</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;'>
                                        {tar.Owner}
                                    </td>
                                </tr>
                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;width:220px;'>
                                        <b>Thời gian/ địa điểm</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;'>
                                        {tar.Location} / {tar.DateCreate:dd/MM/yyyy HH:mm}
                                    </td>
                                </tr>
                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;width:220px;'>
                                        <b>Vấn đề phát hiện</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;'>
                                        {tar.Note}
                                    </td>
                                </tr>
                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;width:220px;'>
                                        <b>Người xử lý</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;'>
                                        {tar.Assignee}
                                    </td>
                                </tr>

                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;'>
                                        <b>Thời gian hoàn thành</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;'>
                                        {tar.DoneTime:dd/MM/yyyy HH:mm}
                                    </td>
                                </tr>

                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;'>
                                        <b>Biện pháp xử lý</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;'>
                                        {req.ActionPlan}
                                    </td>
                                </tr>

                                <tr>
                                    <td style='border:1px solid #d9d9d9;padding:10px;background:#f5f5f5;'>
                                        <b>Trạng thái</b>
                                    </td>
                                    <td style='border:1px solid #d9d9d9;padding:10px;color:green;font-weight:bold;'>
                                        Đã hoàn thành
                                    </td>
                                </tr>
                            </table>

                            <p style='margin-top:20px;'>
                                Vui lòng đăng nhập hệ thống Audit để xem chi tiết và đánh giá kết quả xử lý.
                            </p>

                            <div style='margin-top:30px;padding-top:15px;border-top:1px solid #e8e8e8;color:#666;'>
                                Trân trọng,<br/>
                                <b>Audit System</b><br/>
                                Liên Việt Logistics
                            </div>

                        </div>

                    </div>";

                    await SendMail(
                        userTo.Email,
                        userCC.Email,
                        "[AUDIT SYSTEM] Công việc Audit đã hoàn thành",
                        body);


                    res.Message = "Xác nhận xử lý công việc thành công";
                    res.Status = "SUCCESS";
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<CommonResponseModel<PagingResponse<AuditDetailResponse>>> GetAuditOwner(GetAuditOwner req)
        {
            CommonResponseModel<PagingResponse<AuditDetailResponse>> res = new CommonResponseModel<PagingResponse<AuditDetailResponse>>();
            try
            {
                IQueryable<AuditDetail> query;

                if (req.Status == "Đã phân công")
                {
                    query = _context.AuditDetails.Where(x => (x.Owner.Contains(req.Owner) || req.Owner == "")
                    && x.Status == "Phát hành"
                    && (x.Assignee != null && x.Assignee != "")).OrderByDescending(x => x.Id).AsQueryable();
                }
                else {
                    query = _context.AuditDetails.Where(x => (x.Owner.Contains(req.Owner) || req.Owner == "")
                         && x.Status == "Mới phát hiện").OrderByDescending(x => x.Id).AsQueryable();
                }



                var totalRecords = query.Count();

                var users = await query
                .Skip((req.PageIndex - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var data = _mapper.Map<List<AuditDetailResponse>>(users);

                var paging = new PagingResponse<AuditDetailResponse>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = req.PageIndex,
                    PageSize = req.PageSize
                };

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.Data = paging;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> PhanCongTask(PhanCong req)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();

            try
            {
                AuditDetail tr = await _context.AuditDetails
                    .FirstOrDefaultAsync(x => x.Id == req.IdAuditDetail);

                if (tr == null)
                {
                    res.Message = "Không tìm thấy thông tin vấn đề";
                    res.Status = "ERROR";
                    return res;
                }

                tr.Assignee = req.Asignee;
                tr.Status = "Phát hành";
                tr.DueDate = req.DueDate;

                await _context.SaveChangesAsync();

                var userTo = await _context.Users
                    .FirstOrDefaultAsync(x => x.FullName == req.Asignee);

                var userCC = await _context.Users
                    .FirstOrDefaultAsync(x => x.FullName == tr.Owner);

                // Gửi mail
                if (userTo != null && !string.IsNullOrEmpty(userTo.Email))
                {
                    string body = $@"
                        <div style='font-family:Segoe UI, Arial, sans-serif; font-size:14px; color:#333;'>

                            <div style='background:#1F4E79;color:#fff;padding:15px 20px;border-radius:6px 6px 0 0;'>
                                <h2 style='margin:0;'>THÔNG BÁO PHÂN CÔNG AUDIT TASK</h2>
                            </div>

                            <div style='border:1px solid #d9d9d9;padding:20px;border-radius:0 0 6px 6px;'>

                                <p>Kính gửi <b>{tr.Assignee}</b>,</p>

                                <p>
                                    Hệ thống vừa ghi nhận một nhiệm vụ Audit mới được phân công đến Quý Anh/Chị.
                                    Vui lòng xem thông tin chi tiết bên dưới và thực hiện xử lý theo đúng thời hạn.
                                </p>

                                <table style='border-collapse:collapse;width:100%;margin-top:15px;'>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;width:180px;'>
                                            Nội dung vấn đề
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {tr.Note}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;width:180px;'>
                                            Vi phạm mục
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {tr.OperationStandard} - {tr.DetechIssue}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Địa điểm / Thời gian
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {tr.Location} / {tr.DateCreate:dd/MM/yyyy}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Người phát hiện
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {tr.Owner}
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Người phụ trách
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;'>
                                            {tr.Assignee}
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Hạn hoàn thành
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;color:#C00000;font-weight:bold;'>
                                            {tr.DueDate:dd/MM/yyyy}
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:10px;background:#f5f5f5;border:1px solid #ddd;font-weight:bold;'>
                                            Trạng thái
                                        </td>
                                        <td style='padding:10px;border:1px solid #ddd;color:#C00000;font-weight:bold;'>
                                            {tr.Status}
                                        </td>
                                    </tr>

                                </table>

                                <p style='margin-top:20px;'>
                                    Đề nghị Quý Anh/Chị đăng nhập vào hệ thống Audit để cập nhật nguyên nhân,
                                    hành động khắc phục và tiến độ thực hiện.
                                </p>

                                <p>
                                    Trường hợp đã hoàn thành xử lý, vui lòng cập nhật trạng thái trên hệ thống để phục vụ công tác theo dõi và đánh giá.
                                </p>

                                <hr style='margin:25px 0;border:none;border-top:1px solid #ddd;'>

                                <p style='font-size:12px;color:#666;'>
                                    Đây là thư thông báo được gửi tự động từ hệ thống Audit.<br/>
                                    Vui lòng không trả lời thư điện tử này.
                                </p>

                            </div>
                        </div>";

                    await SendMail(
                        userTo.Email,
                        userCC.Email,
                        "Audit Task Assignment",
                        body);
                }

                res.Message = "Phân công thành công";
                res.Status = "SUCCESS";
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task SendMail(string toEmail, string ccMail, string subject, string body)
        {
            try
            {
                using var mail = new MailMessage();

                mail.From = new MailAddress(
                    _emailSettings.Email,
                    _emailSettings.DisplayName);

                mail.To.Add(toEmail);

                if (!string.IsNullOrWhiteSpace(ccMail))
                {
                    mail.Bcc.Add(ccMail);
                }

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using var client = new SmtpClient("smtp.gmail.com", 587);

                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(
                _emailSettings.Email,
                _emailSettings.Password);

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                throw new Exception($"Send mail failed: {ex.Message}");
            }
        }



        public async Task<CommonResponseModel<CtpatCriteriaResponse>> GetCtpatCriteria()
        {
            CommonResponseModel<CtpatCriteriaResponse> res = new CommonResponseModel<CtpatCriteriaResponse>();
            try
            {
                List<CtpatTopic> lst = await _context.CtpatTopics.ToListAsync();
                List<CtpatCriteriaResponse> lstRes = new List<CtpatCriteriaResponse>();

                List<CtpatCriterion> criterials = await _context.CtpatCriteria.ToListAsync();

                lst.ForEach(async topi => {
                    CtpatCriteriaResponse item = new CtpatCriteriaResponse();
                    item.IDTopic = topi.Id;
                    item.Topic = topi.Topic;
                    item.TopicNote = topi.Note;
                    item.Criterias = criterials.Where(x => x.Idtopic == topi.Id).ToList();
                    lstRes.Add(item);
                });

                res.Message = "lấy bộ tiêu chí C-TPAT thành công";
                res.Status = "SUCCESS";
                res.ListData = lstRes;
                return res;
            }
            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }
    }
}
