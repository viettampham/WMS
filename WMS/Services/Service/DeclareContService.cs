using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using WMS.Models;
using WMS.Models.Request.Cont;
using WMS.Models.Response.Common;
using WMS.Models.Response.Cont;
using WMS.Models.Response.User;
using WMS.Services.impl;
using System.IO.Compression;


namespace WMS.Services.Service
{
    public class DeclareContService : IDeclareContService
    {
        private readonly LienVietStorageContext _context;
        private readonly IMapper _mapper;
        public DeclareContService(LienVietStorageContext context, IMapper mapper) { 
            _context = context;
            _mapper = mapper;
        }

        public async Task<CommonResponseModel<ConImageResponse>> AddImageToCont(AddImageToContRequest req)
        {
            CommonResponseModel<ConImageResponse> res = new CommonResponseModel<ConImageResponse>();
            try
            {
                Cont checkCont = await _context.Conts.Where(x => x.Id == req.IDCont).FirstOrDefaultAsync();
                if (checkCont == null)
                {
                    res.Message = "Không tồn tại thông tin cont";
                    res.Status = "ERROR";
                    return res;
                }

                // lưu ảnh vào folder;
                PathConfig pathConfig = await _context.PathConfigs.Where(x=>x.PathName == "ContImage").FirstOrDefaultAsync();
                if (pathConfig == null) {
                    res.Message = "Chưa cấu hình pathconfig cho 'ContImage'";
                    res.Status = "WARNING";
                    return res;
                }

                string folderPath = pathConfig.PathLocation;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string extension = Path.GetExtension(req.Image.FileName);
                string fileName = $"{req.IDCont}_{Guid.NewGuid()}{extension}";

                string pathSave = Path.Combine(checkCont.SoBooking, checkCont.SoCont, fileName);
                string filePath = Path.Combine(folderPath, pathSave);

                string directoryPath = Path.GetDirectoryName(filePath);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(filePath,FileMode.Create))
                {
                    await req.Image.CopyToAsync(stream);
                }
                string imagePath = Path.Combine("ContImages",fileName);
                ContImage newi = new ContImage();
                newi.Idcont = req.IDCont;
                newi.Path = pathSave;
                newi.NguoiChup = req.NguoiChup;
                newi.ThoiGianChup = DateTime.Now;

                checkCont.SoLuongAnh = checkCont.SoLuongAnh + 1;
                _context.ContImages.Add(newi);
                await _context.SaveChangesAsync();

                res.Message = "Thêm ảnh thành công";
                res.Status = "SUCCESS";
                res.ListData = await _context.ContImages.Where(x => x.Idcont == req.IDCont).Select(x=>new ConImageResponse
                {
                    ID = x.Id,
                    IDCont = x.Idcont,
                    NguoiChup = x.NguoiChup,
                    ThoiGianChup = x.ThoiGianChup,
                    Path = x.Path
                }).ToListAsync();
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<ConImageResponseWithIDCont>> Declare(DeclareContRequest req)
        {
            CommonResponseModel<ConImageResponseWithIDCont> res = new CommonResponseModel<ConImageResponseWithIDCont>();
            try
            {
                Cont checkcont = await _context.Conts.Where(x => x.SoBooking == req.SoBooking && x.SoCont == req.SoCont).FirstOrDefaultAsync();
                if (checkcont == null)
                {
                    Cont newi = new Cont();
                    newi.SoBooking = req.SoBooking;
                    newi.SoCont = req.SoCont;
                    newi.SoLuongAnh = 0;
                    newi.NguoiKhaiBao = req.NguoiKhaiBao;

                    _context.Conts.Add(newi);
                    await _context.SaveChangesAsync();

                    res.Message = "Khai báo thành công";
                    res.Status = "SUCCESS";
                    ConImageResponseWithIDCont datares = new ConImageResponseWithIDCont();
                    datares.IDCont = newi.Id;
                    datares.LstDataImage = null;
                    res.Data = datares;
                    return res;
                }
                else {
                    res.Message = "Khai báo thành công";
                    res.Status = "SUCCESS";
                    List<ConImageResponse> lst = _context.ContImages
                        .Where(x => x.Idcont == checkcont.Id)
                        .Select(x => new ConImageResponse
                        {
                            ID = x.Id,
                            Path = x.Path,
                            ThoiGianChup = x.ThoiGianChup,
                            NguoiChup = x.NguoiChup,
                            IDCont = x.Idcont
                        })
                        .ToList();
                    ConImageResponseWithIDCont datares = new ConImageResponseWithIDCont();
                    datares.IDCont = checkcont.Id;
                    datares.LstDataImage = lst;
                    res.Data = datares;
                    return res;
                }
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }


        public async Task<CommonResponseModel<string>> DowloadFolder(int id){
            CommonResponseModel<string> res = new CommonResponseModel<string>();

            try
            {
                Cont checkcont = await _context.Conts.Where(x => x.Id == id).FirstOrDefaultAsync();

                if (checkcont == null) {
                    res.Message = "Không tìm thấy thông tin dữ liệu";
                    res.Status = "ERROR";
                    return res;
                }
                PathConfig pathConfig = await _context.PathConfigs.Where(x=>x.PathName == "ContImage").FirstOrDefaultAsync();
                string pathFolder = Path.Combine(pathConfig.PathLocation, checkcont.SoBooking, checkcont.SoCont);

                if (!Directory.Exists(pathFolder))
                {
                    res.Status = "ERROR";
                    res.Message = "Không tìm thấy thư mục";
                    return res;
                }

                var zipFileName = $"{Guid.NewGuid()}.zip";
                var zipPath = Path.Combine(Path.GetTempPath(), zipFileName);

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                ZipFile.CreateFromDirectory(pathFolder, zipPath);

                res.Status = "SUCCESS";
                res.Data = zipPath;
                res.Message = "Tạo file zip thành công";

                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Message = "Lỗi " + ex.Message;
                return res;
            }
        }

        public async Task<(byte[] FileBytes, string FileName)> DownloadFolder(int id)
        {
            var checkcont = await _context.Conts
                .FirstOrDefaultAsync(x => x.Id == id);

            if (checkcont == null)
                throw new Exception("Không tìm thấy container");

            var pathConfig = await _context.PathConfigs
                .FirstOrDefaultAsync(x => x.PathName == "ContImage");

            if (pathConfig == null)
                throw new Exception("Không tìm thấy cấu hình đường dẫn");

            var pathFolder = Path.Combine(
                pathConfig.PathLocation,
                checkcont.SoBooking,
                checkcont.SoCont
            );

            if (!Directory.Exists(pathFolder))
                throw new Exception("Không tìm thấy thư mục ảnh");

            var zipFileName = $"{checkcont.SoBooking}_{checkcont.SoCont}.zip";

            var zipPath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}_{zipFileName}"
            );

            ZipFile.CreateFromDirectory(pathFolder, zipPath);

            var fileBytes = await File.ReadAllBytesAsync(zipPath);

            // Xóa file tạm sau khi đọc
            File.Delete(zipPath);

            return (fileBytes, zipFileName);
        }

        public async Task<CommonResponseModel<PagingResponse<DeclareContResponse>>> GetList(GetDeclateContRequest req)
            {
                CommonResponseModel<PagingResponse<DeclareContResponse>> res = new CommonResponseModel<PagingResponse<DeclareContResponse>>();
                try {
                    var query = _context.Conts.Where(x => (x.SoBooking.Contains(req.SoBooking) || req.SoBooking== "")
                    && (x.SoCont.Contains(req.SoCont) || req.SoCont== "")).AsQueryable();

                    var totalRecords = query.Count();

                    var conts = await query
                    .Skip((req.PageIndex - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync();

                    var data = _mapper.Map<List<DeclareContResponse>>(conts);

                    var paging = new PagingResponse<DeclareContResponse>
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
        }
}
