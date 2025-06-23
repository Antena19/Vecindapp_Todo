using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;
using REST_VECINDAPP.Modelos;
using REST_VECINDAPP.Modelos.DTOs;
using Microsoft.AspNetCore.Http;

namespace REST_VECINDAPP.CapaNegocios
{
    public class cn_Comunicacion
    {
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public cn_Comunicacion(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _httpContextAccessor = httpContextAccessor;
        }

        #region NOTICIAS

        public async Task<List<Noticia>> ObtenerNoticiasAsync(string? categoria = null, string? alcance = null, 
            string? estado = null, int? autorRut = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var noticias = new List<Noticia>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT n.id, n.titulo, n.contenido, n.fecha_publicacion, n.fecha_evento,
                           n.autor_rut, n.autor_nombre, n.alcance, n.prioridad, n.estado,
                           n.imagen, n.categoria, n.tags
                    FROM noticias n
                    WHERE 1=1";
                
                var parameters = new List<MySqlParameter>();
                
                if (!string.IsNullOrEmpty(categoria))
                {
                    query += " AND n.categoria = @Categoria";
                    parameters.Add(new MySqlParameter("@Categoria", categoria));
                }
                
                if (!string.IsNullOrEmpty(alcance))
                {
                    query += " AND n.alcance = @Alcance";
                    parameters.Add(new MySqlParameter("@Alcance", alcance));
                }
                
                if (!string.IsNullOrEmpty(estado))
                {
                    query += " AND n.estado = @Estado";
                    parameters.Add(new MySqlParameter("@Estado", estado));
                }
                
                if (autorRut.HasValue)
                {
                    query += " AND n.autor_rut = @AutorRut";
                    parameters.Add(new MySqlParameter("@AutorRut", autorRut.Value));
                }
                
                if (fechaDesde.HasValue)
                {
                    query += " AND n.fecha_publicacion >= @FechaDesde";
                    parameters.Add(new MySqlParameter("@FechaDesde", fechaDesde.Value));
                }
                
                if (fechaHasta.HasValue)
                {
                    query += " AND n.fecha_publicacion <= @FechaHasta";
                    parameters.Add(new MySqlParameter("@FechaHasta", fechaHasta.Value));
                }
                
                query += " ORDER BY n.fecha_publicacion DESC";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            noticias.Add(new Noticia
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Contenido = reader.GetString("contenido"),
                                FechaCreacion = reader.GetDateTime("fecha_publicacion"),
                                FechaPublicacion = reader.IsDBNull("fecha_publicacion") ? null : reader.GetDateTime("fecha_publicacion"),
                                AutorRut = reader.GetInt32("autor_rut"),
                                AutorNombre = reader.IsDBNull("autor_nombre") ? null : reader.GetString("autor_nombre"),
                                Alcance = reader.GetString("alcance"),
                                Prioridad = reader.GetString("prioridad"),
                                Estado = reader.GetString("estado"),
                                ImagenUrl = reader.IsDBNull("imagen") ? null : reader.GetString("imagen"),
                                Categoria = reader.GetString("categoria"),
                                Tags = reader.IsDBNull("tags") ? null : reader.GetString("tags")
                            });
                        }
                    }
                }
            }
            
            return noticias;
        }

        public async Task<Noticia?> ObtenerNoticiaPorIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT id, titulo, contenido, fecha_publicacion, fecha_evento,
                           autor_rut, autor_nombre, alcance, prioridad, estado,
                           imagen, categoria, tags
                    FROM noticias
                    WHERE id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Noticia
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Contenido = reader.GetString("contenido"),
                                FechaCreacion = reader.GetDateTime("fecha_publicacion"),
                                FechaPublicacion = reader.IsDBNull("fecha_publicacion") ? null : reader.GetDateTime("fecha_publicacion"),
                                AutorRut = reader.GetInt32("autor_rut"),
                                AutorNombre = reader.IsDBNull("autor_nombre") ? null : reader.GetString("autor_nombre"),
                                Alcance = reader.GetString("alcance"),
                                Prioridad = reader.GetString("prioridad"),
                                Estado = reader.GetString("estado"),
                                ImagenUrl = reader.IsDBNull("imagen") ? null : reader.GetString("imagen"),
                                Categoria = reader.GetString("categoria"),
                                Tags = reader.IsDBNull("tags") ? null : reader.GetString("tags")
                            };
                        }
                    }
                }
            }
            
            return null;
        }

        public async Task<Noticia> CrearNoticiaAsync(CrearNoticiaDTO dto)
        {
            // Obtener información del usuario actual desde el contexto inyectado
            var userRut = _httpContextAccessor.HttpContext?.User?.FindFirst("Rut")?.Value
                        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("rut")?.Value
                        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value;

            var userNombre = _httpContextAccessor.HttpContext?.User?.FindFirst("nombre")?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value
                           ?? "Desconocido";
            
            if (string.IsNullOrEmpty(userRut) || string.IsNullOrEmpty(userNombre))
                throw new Exception("No se pudo obtener la información del usuario");
            
            return await CrearNoticiaAsync(dto, int.Parse(userRut), userNombre);
        }

        public async Task<Noticia> CrearNoticiaAsync(CrearNoticiaDTO dto, int autorRut, string autorNombre)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    INSERT INTO noticias (titulo, contenido, fecha_publicacion, autor_rut, autor_nombre, 
                                         alcance, prioridad, estado, categoria, tags)
                    VALUES (@Titulo, @Contenido, @FechaCreacion, @AutorRut, @AutorNombre, 
                           @Alcance, @Prioridad, 'ACTIVO', @Categoria, @Tags);
                    SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Titulo", dto.Titulo);
                    command.Parameters.AddWithValue("@Contenido", dto.Contenido);
                    command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                    command.Parameters.AddWithValue("@AutorRut", autorRut);
                    command.Parameters.AddWithValue("@AutorNombre", autorNombre);
                    command.Parameters.AddWithValue("@Alcance", dto.Alcance);
                    command.Parameters.AddWithValue("@Prioridad", dto.Prioridad);
                    command.Parameters.AddWithValue("@Categoria", dto.Categoria);
                    command.Parameters.AddWithValue("@Tags", 
                        dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : (object)DBNull.Value);
                    
                    var id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    
                    return new Noticia
                    {
                        Id = id,
                        Titulo = dto.Titulo,
                        Contenido = dto.Contenido,
                        FechaCreacion = DateTime.Now,
                        AutorRut = autorRut,
                        AutorNombre = autorNombre,
                        Alcance = dto.Alcance,
                        Prioridad = dto.Prioridad,
                        Estado = "ACTIVO",
                        Categoria = dto.Categoria,
                        Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null
                    };
                }
            }
        }

        public async Task<Noticia> ActualizarNoticiaAsync(int id, ActualizarNoticiaDTO dto)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Obtener la noticia existente para conservar la fecha si es necesario
                var noticiaExistente = await ObtenerNoticiaPorIdAsync(id);
                var fechaPublicacion = dto.PublicarInmediatamente
                    ? DateTime.Now
                    : noticiaExistente?.FechaPublicacion ?? DateTime.Now;
                
                var query = @"
                    UPDATE noticias 
                    SET titulo = @Titulo, contenido = @Contenido, alcance = @Alcance,
                        prioridad = @Prioridad, estado = @Estado, categoria = @Categoria,
                        tags = @Tags, fecha_publicacion = @FechaPublicacion
                    WHERE id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Titulo", dto.Titulo);
                    command.Parameters.AddWithValue("@Contenido", dto.Contenido);
                    command.Parameters.AddWithValue("@Alcance", dto.Alcance);
                    command.Parameters.AddWithValue("@Prioridad", dto.Prioridad);
                    command.Parameters.AddWithValue("@Estado", dto.PublicarInmediatamente ? "ACTIVO" : "INACTIVO");
                    command.Parameters.AddWithValue("@Categoria", dto.Categoria);
                    command.Parameters.AddWithValue("@Tags", 
                        dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FechaPublicacion", fechaPublicacion);
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                // Obtener la noticia actualizada
                return await ObtenerNoticiaPorIdAsync(id) ?? throw new Exception("No se pudo actualizar la noticia");
            }
        }

        public async Task<bool> EliminarNoticiaAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "DELETE FROM noticias WHERE id = @Id";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Noticia> PublicarNoticiaAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    UPDATE noticias 
                    SET estado = 'ACTIVO', fecha_publicacion = @FechaPublicacion
                    WHERE id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@FechaPublicacion", DateTime.Now);
                    
                    await command.ExecuteNonQueryAsync();
                }
                
                return await ObtenerNoticiaPorIdAsync(id) ?? throw new Exception("No se pudo publicar la noticia");
            }
        }

        public async Task<List<Noticia>> ObtenerNoticiasDestacadasAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT id, titulo, contenido, fecha_publicacion, fecha_evento,
                           autor_rut, autor_nombre, alcance, prioridad, estado,
                           imagen, categoria, tags
                    FROM noticias
                    WHERE estado = 'ACTIVO'
                    ORDER BY prioridad DESC, fecha_publicacion DESC
                    LIMIT 5";
                
                var noticias = new List<Noticia>();
                
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var contenido = reader.GetString("contenido");
                            var resumen = GenerarResumen(contenido);
                            
                            noticias.Add(new Noticia
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Contenido = contenido,
                                Resumen = resumen,
                                FechaCreacion = reader.GetDateTime("fecha_publicacion"),
                                FechaPublicacion = reader.IsDBNull("fecha_publicacion") ? null : reader.GetDateTime("fecha_publicacion"),
                                AutorRut = reader.GetInt32("autor_rut"),
                                AutorNombre = reader.IsDBNull("autor_nombre") ? null : reader.GetString("autor_nombre"),
                                Alcance = reader.GetString("alcance"),
                                Prioridad = reader.GetString("prioridad"),
                                Estado = reader.GetString("estado"),
                                ImagenUrl = reader.IsDBNull("imagen") ? null : reader.GetString("imagen"),
                                Categoria = reader.GetString("categoria"),
                                Tags = reader.IsDBNull("tags") ? null : reader.GetString("tags")
                            });
                        }
                    }
                }
                
                return noticias;
            }
        }

        private string GenerarResumen(string contenido)
        {
            // Limpiar HTML tags si existen
            var textoLimpio = System.Text.RegularExpressions.Regex.Replace(contenido, "<[^>]*>", "");
            
            // Tomar los primeros 150 caracteres
            if (textoLimpio.Length <= 150)
                return textoLimpio;
            
            // Buscar el último espacio antes de los 150 caracteres para no cortar palabras
            var resumen = textoLimpio.Substring(0, 150);
            var ultimoEspacio = resumen.LastIndexOf(' ');
            
            if (ultimoEspacio > 0)
                resumen = resumen.Substring(0, ultimoEspacio);
            
            return resumen + "...";
        }

        public async Task<string> SubirImagenNoticiaAsync(int noticiaId, IFormFile imagen)
        {
            // Generar nombre único para la imagen
            var extension = Path.GetExtension(imagen.FileName);
            var nombreArchivo = $"noticia_{noticiaId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var rutaDestino = Path.Combine("wwwroot", "archivos", "noticias", nombreArchivo);
            
            // Crear directorio si no existe
            var directorio = Path.GetDirectoryName(rutaDestino);
            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio!);
            }
            
            // Guardar archivo
            using (var stream = new FileStream(rutaDestino, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }
            
            // Actualizar URL en la base de datos
            var imagenUrl = $"/archivos/noticias/{nombreArchivo}";
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = "UPDATE noticias SET imagen = @ImagenUrl WHERE id = @Id";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", noticiaId);
                    command.Parameters.AddWithValue("@ImagenUrl", imagenUrl);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
            
            return imagenUrl;
        }

        #endregion

        #region COMENTARIOS

        public async Task<List<ComentarioNoticia>> ObtenerComentariosNoticiaAsync(int noticiaId)
        {
            var comentarios = new List<ComentarioNoticia>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT c.id, c.noticia_id, c.usuario_rut, c.usuario_nombre,
                           c.contenido, c.fecha_creacion, c.estado
                    FROM comentarios_noticia c
                    WHERE c.noticia_id = @NoticiaId AND c.estado = 'ACTIVO'
                    ORDER BY c.fecha_creacion DESC";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NoticiaId", noticiaId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            comentarios.Add(new ComentarioNoticia
                            {
                                Id = reader.GetInt32("id"),
                                NoticiaId = reader.GetInt32("noticia_id"),
                                UsuarioRut = reader.GetInt32("usuario_rut"),
                                UsuarioNombre = reader.GetString("usuario_nombre"),
                                Contenido = reader.GetString("contenido"),
                                FechaCreacion = reader.GetDateTime("fecha_creacion"),
                                Estado = reader.GetString("estado")
                            });
                        }
                    }
                }
            }
            
            return comentarios;
        }

        public async Task<ComentarioNoticia> CrearComentarioAsync(int noticiaId, CrearComentarioDTO dto)
        {
            // Obtener nombre del usuario
            var usuarioNombre = await ObtenerNombreUsuarioAsync(dto.UsuarioRut);
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    INSERT INTO comentarios_noticia (noticia_id, usuario_rut, usuario_nombre, 
                                                  contenido, fecha_creacion, estado)
                    VALUES (@NoticiaId, @UsuarioRut, @UsuarioNombre, @Contenido, @FechaCreacion, 'ACTIVO');
                    SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NoticiaId", noticiaId);
                    command.Parameters.AddWithValue("@UsuarioRut", dto.UsuarioRut);
                    command.Parameters.AddWithValue("@UsuarioNombre", usuarioNombre);
                    command.Parameters.AddWithValue("@Contenido", dto.Contenido);
                    command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                    
                    var id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    
                    return new ComentarioNoticia
                    {
                        Id = id,
                        NoticiaId = noticiaId,
                        UsuarioRut = dto.UsuarioRut,
                        UsuarioNombre = usuarioNombre,
                        Contenido = dto.Contenido,
                        FechaCreacion = DateTime.Now,
                        Estado = "ACTIVO"
                    };
                }
            }
        }

        public async Task<bool> EliminarComentarioAsync(int id)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "DELETE FROM comentarios_noticia WHERE id = @Id";
                    
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region NOTIFICACIONES

        public async Task<List<NotificacionUsuario>> ObtenerNotificacionesUsuarioAsync(int usuarioRut)
        {
            var notificaciones = new List<NotificacionUsuario>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT nu.Id, nu.NotificacionId, nu.UsuarioRut, nu.Leida,
                           nu.FechaLectura, nu.FechaRecepcion,
                           n.Titulo, n.Mensaje, n.Tipo, n.Prioridad
                    FROM NotificacionesUsuario nu
                    INNER JOIN Notificaciones n ON nu.NotificacionId = n.Id
                    WHERE nu.UsuarioRut = @UsuarioRut
                    ORDER BY nu.FechaRecepcion DESC";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UsuarioRut", usuarioRut);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            notificaciones.Add(new NotificacionUsuario
                            {
                                Id = reader.GetInt32("Id"),
                                NotificacionId = reader.GetInt32("NotificacionId"),
                                UsuarioRut = reader.GetInt32("UsuarioRut"),
                                Leida = reader.GetBoolean("Leida"),
                                FechaLectura = reader.IsDBNull("FechaLectura") ? null : reader.GetDateTime("FechaLectura"),
                                FechaRecepcion = reader.GetDateTime("FechaRecepcion")
                            });
                        }
                    }
                }
            }
            
            return notificaciones;
        }

        public async Task MarcarNotificacionLeidaAsync(int notificacionId, int usuarioRut)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    UPDATE NotificacionesUsuario 
                    SET Leida = 1, FechaLectura = @FechaLectura
                    WHERE NotificacionId = @NotificacionId AND UsuarioRut = @UsuarioRut";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NotificacionId", notificacionId);
                    command.Parameters.AddWithValue("@UsuarioRut", usuarioRut);
                    command.Parameters.AddWithValue("@FechaLectura", DateTime.Now);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Notificacion> CrearNotificacionAsync(string titulo, string mensaje, string tipo, 
            string prioridad, List<int> destinatarios, int? noticiaId = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    INSERT INTO Notificaciones (Titulo, Mensaje, FechaCreacion, Tipo, Estado, 
                                              Destinatarios, NoticiaId, Prioridad)
                    VALUES (@Titulo, @Mensaje, @FechaCreacion, @Tipo, 'PENDIENTE', 
                           @Destinatarios, @NoticiaId, @Prioridad);
                    SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Titulo", titulo);
                    command.Parameters.AddWithValue("@Mensaje", mensaje);
                    command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                    command.Parameters.AddWithValue("@Tipo", tipo);
                    command.Parameters.AddWithValue("@Destinatarios", JsonSerializer.Serialize(destinatarios));
                    command.Parameters.AddWithValue("@NoticiaId", noticiaId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Prioridad", prioridad);
                    
                    var id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    
                    // Crear registros de notificación para cada destinatario
                    foreach (var destinatario in destinatarios)
                    {
                        await CrearNotificacionUsuarioAsync(id, destinatario);
                    }
                    
                    return new Notificacion
                    {
                        Id = id,
                        Titulo = titulo,
                        Mensaje = mensaje,
                        FechaCreacion = DateTime.Now,
                        Tipo = tipo,
                        Estado = "PENDIENTE",
                        Destinatarios = JsonSerializer.Serialize(destinatarios),
                        NoticiaId = noticiaId,
                        Prioridad = prioridad
                    };
                }
            }
        }

        private async Task CrearNotificacionUsuarioAsync(int notificacionId, int usuarioRut)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    INSERT INTO NotificacionesUsuario (NotificacionId, UsuarioRut, Leida, FechaRecepcion)
                    VALUES (@NotificacionId, @UsuarioRut, 0, @FechaRecepcion)";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NotificacionId", notificacionId);
                    command.Parameters.AddWithValue("@UsuarioRut", usuarioRut);
                    command.Parameters.AddWithValue("@FechaRecepcion", DateTime.Now);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        #endregion

        #region ESTADÍSTICAS

        public async Task<EstadisticasComunicacion> ObtenerEstadisticasComunicacionAsync()
        {
            var estadisticas = new EstadisticasComunicacion();
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Total de noticias publicadas
                using (var command = new MySqlCommand("SELECT COUNT(*) FROM noticias WHERE estado = 'ACTIVO'", connection))
                {
                    estadisticas.NoticiasPublicadas = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                // Total de lecturas
                using (var command = new MySqlCommand("SELECT COUNT(*) FROM lecturas_noticia", connection))
                {
                    estadisticas.TotalLecturas = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                // Total de comentarios
                using (var command = new MySqlCommand("SELECT COUNT(*) FROM comentarios_noticia WHERE estado = 'ACTIVO'", connection))
                {
                    estadisticas.TotalComentarios = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                // Tasa de apertura/lectura
                if (estadisticas.NoticiasPublicadas > 0)
                {
                    // Calcular cuántas noticias han sido leídas al menos una vez
                    using (var command = new MySqlCommand("SELECT COUNT(DISTINCT noticia_id) FROM lecturas_noticia", connection))
                    {
                        var noticiasLeidas = Convert.ToInt32(await command.ExecuteScalarAsync());
                        estadisticas.TasaLectura = (double)noticiasLeidas / estadisticas.NoticiasPublicadas * 100;
                    }
                }
            }
            return estadisticas;
        }

        #endregion

        #region MÉTODOS AUXILIARES

        private async Task<string> ObtenerNombreUsuarioAsync(int rut)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = "SELECT CONCAT(nombre, ' ', apellido) FROM usuarios WHERE rut = @Rut";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Rut", rut);
                    
                    var resultado = await command.ExecuteScalarAsync();
                    return resultado?.ToString() ?? "Usuario";
                }
            }
        }

        public async Task<List<int>> ObtenerRutsUsuariosAsync()
        {
            var ruts = new List<int>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = "SELECT rut FROM usuarios WHERE estado = 'ACTIVO'";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ruts.Add(reader.GetInt32("rut"));
                        }
                    }
                }
            }
            
            return ruts;
        }

        #endregion

        public async Task<Noticia?> ObtenerNoticiaAsync(int id)
        {
            return await ObtenerNoticiaPorIdAsync(id);
        }

        public async Task<List<Noticia>> BuscarNoticiasAsync(string termino)
        {
            var noticias = new List<Noticia>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT id, titulo, contenido, fecha_publicacion, fecha_evento,
                           autor_rut, autor_nombre, alcance, prioridad, estado,
                           imagen, categoria, tags
                    FROM noticias
                    WHERE estado = 'ACTIVO' 
                    AND (titulo LIKE @Termino OR contenido LIKE @Termino)
                    ORDER BY fecha_publicacion DESC";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Termino", $"%{termino}%");
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            noticias.Add(new Noticia
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Contenido = reader.GetString("contenido"),
                                FechaCreacion = reader.GetDateTime("fecha_publicacion"),
                                FechaPublicacion = reader.IsDBNull("fecha_publicacion") ? null : reader.GetDateTime("fecha_publicacion"),
                                AutorRut = reader.GetInt32("autor_rut"),
                                AutorNombre = reader.IsDBNull("autor_nombre") ? null : reader.GetString("autor_nombre"),
                                Alcance = reader.GetString("alcance"),
                                Prioridad = reader.GetString("prioridad"),
                                Estado = reader.GetString("estado"),
                                ImagenUrl = reader.IsDBNull("imagen") ? null : reader.GetString("imagen"),
                                Categoria = reader.GetString("categoria"),
                                Tags = reader.IsDBNull("tags") ? null : reader.GetString("tags")
                            });
                        }
                    }
                }
            }
            
            return noticias;
        }

        public async Task<ComentarioNoticia> AgregarComentarioAsync(int noticiaId, CrearComentarioDTO dto)
        {
            return await CrearComentarioAsync(noticiaId, dto);
        }

        public async Task<Notificacion> CrearNotificacionAsync(CrearNotificacionDTO dto)
        {
            return await CrearNotificacionAsync(dto.Titulo, dto.Mensaje, dto.Tipo, dto.Prioridad, dto.Destinatarios, dto.NoticiaId);
        }

        public async Task<bool> MarcarNotificacionLeidaAsync(int id)
        {
            try
            {
                var httpContextAccessor = new HttpContextAccessor();
                var userRut = httpContextAccessor.HttpContext?.User?.FindFirst("rut")?.Value;
                
                if (string.IsNullOrEmpty(userRut))
                    return false;
                
                await MarcarNotificacionLeidaAsync(id, int.Parse(userRut));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Notificacion> NotificarAvisoImportanteAsync(int noticiaId)
        {
            var noticia = await ObtenerNoticiaAsync(noticiaId);
            if (noticia == null)
                throw new Exception("Noticia no encontrada");
            
            var destinatarios = await ObtenerRutsUsuariosAsync();
            
            return await CrearNotificacionAsync(
                $"Aviso Importante: {noticia.Titulo}",
                noticia.Contenido,
                "AVISO_IMPORTANTE",
                noticia.Prioridad,
                destinatarios,
                noticiaId
            );
        }
    }
} 