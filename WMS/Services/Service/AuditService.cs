using AutoMapper;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
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
                if (String.IsNullOrEmpty(req.DueDate))
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

                //await _context.SaveChangesAsync();

                var userTo = await _context.Users
                    .FirstOrDefaultAsync(x => x.FullName == req.Asignee);

                var userCC = await _context.Users
                    .FirstOrDefaultAsync(x => x.FullName == tr.Owner);

                // Gửi mail
                if (userTo != null && !string.IsNullOrEmpty(userTo.Email))
                {
                    string body = $@"
                    <h3>Bạn được phân công Audit Task</h3>

                    <p><b>Vấn đề:</b> {tr.Note}</p>
                    <p><b>Due Date:</b> {tr.DueDate:dd/MM/yyyy}</p>
                    <p><b>Người phân công:</b> {tr.Owner}</p>
                    <p><b>Người đảm nhận:</b> {tr.Assignee}</p>

                    <p>Vui lòng đăng nhập hệ thống để xử lý.</p>";

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

                if (!string.IsNullOrWhiteSpace(ccMail)) { 
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


        public async Task<string> TestSendMail()
        {
            try
            {
                using var mail = new MailMessage();

                mail.From = new MailAddress(
                    _emailSettings.Email,
                    _emailSettings.DisplayName);

                mail.To.Add("it@lienvietlogistics.com.vn");
                mail.CC.Add("phamtampt2k1@gmail.com");
                //mail.CC.Add("it@lienvietlogistics.com.vn");

                mail.Subject = "Audit Task Assignment";
                mail.Body = $@"
                <h3>SMTP Test Success</h3>
                <p>Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>";
                mail.IsBodyHtml = true;

                using var client = new SmtpClient("smtp.gmail.com", 587);

                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(
                _emailSettings.Email,
                _emailSettings.Password);

                await client.SendMailAsync(mail);

                return "Gửi mail thành công";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
