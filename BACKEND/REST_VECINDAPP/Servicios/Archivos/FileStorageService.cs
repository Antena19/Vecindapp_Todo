// REST_VECINDAPP/Servicios/Archivos/FileStorageService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace REST_VECINDAPP.Servicios.Archivos
{
    public class FileStorageService
    {
        private readonly string _basePath;
        private readonly string _identidadPath;
        private readonly string _domicilioPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        public FileStorageService(IConfiguration configuration)
        {
            var fileStorage = configuration.GetSection("FileStorage");

            // Obtener la ruta base desde la configuración
            _basePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "documentos"
            );

            _identidadPath = Path.Combine(_basePath, "identidad");
            _domicilioPath = Path.Combine(_basePath, "domicilio");
            _maxFileSize = 5 * 1024 * 1024; // 5MB por defecto
            _allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            // Crear directorios si no existen
            Directory.CreateDirectory(_identidadPath);
            Directory.CreateDirectory(_domicilioPath);
        }

        public string GuardarDocumento(IFormFile archivo, int rut, string tipoDocumento)
        {
            // Validar tamaño
            if (archivo.Length > _maxFileSize)
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {_maxFileSize / 1024 / 1024}MB");

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"Tipo de archivo no permitido. Tipos permitidos: {string.Join(", ", _allowedExtensions)}");

            // Generar nombre único
            string nombreArchivo = $"{rut}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

            // Determinar ruta según tipo
            string rutaDestino = tipoDocumento.ToLower() == "identidad"
                ? Path.Combine(_identidadPath, nombreArchivo)
                : Path.Combine(_domicilioPath, nombreArchivo);

            // Guardar archivo
            using (var stream = new FileStream(rutaDestino, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            // Retornar ruta relativa para guardar en BD
            return Path.GetRelativePath(
                Directory.GetCurrentDirectory(),
                rutaDestino
            );
        }

        public string ObtenerRutaDocumento(int rut, string tipoDocumento)
        {
            string directorio = tipoDocumento.ToLower() == "identidad" ? _identidadPath : _domicilioPath;
            var archivos = Directory.GetFiles(directorio, $"{rut}_*");

            if (archivos.Length == 0)
                throw new FileNotFoundException($"No se encontró documento de {tipoDocumento} para el RUT {rut}");

            // Retornar el archivo más reciente
            return archivos.OrderByDescending(f => File.GetLastWriteTime(f)).First();
        }
    }
}