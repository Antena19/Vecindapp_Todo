-- =====================================================
-- CORRECCIONES BASE DE DATOS VECINDAPP
-- =====================================================

USE `vecindapp_bd`;

-- =====================================================
-- 1. CORRECCIÓN TABLA NOTICIAS
-- =====================================================

-- Agregar campos faltantes a la tabla noticias (excluyendo fecha_publicacion que ya existe)
ALTER TABLE `noticias` 
ADD COLUMN `alcance` VARCHAR(20) NOT NULL DEFAULT 'PUBLICO' COMMENT 'PUBLICO, SOCIOS',
ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA',
ADD COLUMN `estado` VARCHAR(20) NOT NULL DEFAULT 'ACTIVO' COMMENT 'ACTIVO, INACTIVO',
ADD COLUMN `categoria` VARCHAR(20) NOT NULL DEFAULT 'NOTICIA' COMMENT 'NOTICIA, EVENTO, AVISO',
ADD COLUMN `tags` TEXT NULL COMMENT 'JSON array como string',
ADD COLUMN `autor_nombre` VARCHAR(100) NULL;

-- Actualizar datos existentes (con WHERE para safe update mode)
UPDATE `noticias` SET 
    `alcance` = CASE 
        WHEN `visibilidad` = 'todos' THEN 'PUBLICO'
        WHEN `visibilidad` = 'solo_socios' THEN 'SOCIOS'
        ELSE 'PUBLICO'
    END,
    `estado` = 'ACTIVO',
    `categoria` = 'NOTICIA'
WHERE `id` > 0; -- Usar WHERE con columna KEY para safe update mode

-- Renombrar campos para coincidir con el modelo
ALTER TABLE `noticias` 
CHANGE COLUMN `publicado_por` `autor_rut` INT NOT NULL;

-- Agregar índices para mejor rendimiento
CREATE INDEX `idx_noticias_estado` ON `noticias` (`estado`);
CREATE INDEX `idx_noticias_categoria` ON `noticias` (`categoria`);
CREATE INDEX `idx_noticias_alcance` ON `noticias` (`alcance`);

-- =====================================================
-- 2. CORRECCIÓN TABLA NOTIFICACIONES
-- =====================================================

-- Agregar campos faltantes a la tabla notificaciones
ALTER TABLE `notificaciones` 
ADD COLUMN `fecha_envio` DATETIME NULL,
ADD COLUMN `destinatarios` TEXT NULL COMMENT 'JSON array de RUTs como string',
ADD COLUMN `noticia_id` INT NULL,
ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA',
ADD COLUMN `metadata` TEXT NULL COMMENT 'JSON object como string';

-- Agregar foreign key para noticia_id
ALTER TABLE `notificaciones` 
ADD CONSTRAINT `fk_notificaciones_noticia` 
FOREIGN KEY (`noticia_id`) REFERENCES `noticias` (`id`) ON DELETE SET NULL;

-- Agregar índices
CREATE INDEX `idx_notificaciones_prioridad` ON `notificaciones` (`prioridad`);
CREATE INDEX `idx_notificaciones_noticia_id` ON `notificaciones` (`noticia_id`);

-- =====================================================
-- 3. CREAR TABLA COMENTARIOS_NOTICIA
-- =====================================================

-- Crear tabla comentarios_noticia
CREATE TABLE `comentarios_noticia` (
  `id` int NOT NULL AUTO_INCREMENT,
  `noticia_id` int NOT NULL,
  `usuario_rut` int NOT NULL,
  `usuario_nombre` varchar(100) NOT NULL,
  `contenido` varchar(500) NOT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `estado` varchar(20) NOT NULL DEFAULT 'ACTIVO' COMMENT 'ACTIVO, MODERADO, ELIMINADO',
  PRIMARY KEY (`id`),
  KEY `noticia_id_idx` (`noticia_id`),
  KEY `usuario_rut_idx` (`usuario_rut`),
  KEY `estado_idx` (`estado`),
  CONSTRAINT `fk_comentario_noticia` FOREIGN KEY (`noticia_id`) REFERENCES `noticias` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_comentario_usuario` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =====================================================
-- 4. CREAR TABLA DISPOSITIVOS_USUARIO (OPCIONAL)
-- =====================================================

-- Crear tabla dispositivos_usuario para notificaciones push
CREATE TABLE `dispositivos_usuario` (
  `id` int NOT NULL AUTO_INCREMENT,
  `usuario_rut` int NOT NULL,
  `token_dispositivo` varchar(255) NOT NULL,
  `tipo_dispositivo` varchar(20) NOT NULL COMMENT 'android, ios, web',
  `fecha_registro` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ultima_actividad` datetime NULL,
  `activo` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `token_dispositivo_UNIQUE` (`token_dispositivo`),
  KEY `usuario_rut_idx` (`usuario_rut`),
  CONSTRAINT `fk_dispositivo_usuario` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- =====================================================
-- 5. STORED PROCEDURES ADICIONALES
-- =====================================================

-- SP para obtener estadísticas de noticias
DELIMITER ;;
CREATE PROCEDURE `SP_OBTENER_ESTADISTICAS_NOTICIAS`()
BEGIN
    SELECT 
        COUNT(*) as total_noticias,
        COUNT(CASE WHEN estado = 'ACTIVO' THEN 1 END) as noticias_activas,
        COUNT(CASE WHEN categoria = 'NOTICIA' THEN 1 END) as noticias_generales,
        COUNT(CASE WHEN categoria = 'EVENTO' THEN 1 END) as eventos,
        COUNT(CASE WHEN categoria = 'AVISO' THEN 1 END) as avisos
    FROM noticias;
END ;;
DELIMITER ;

-- SP para obtener comentarios de una noticia
DELIMITER ;;
CREATE PROCEDURE `SP_OBTENER_COMENTARIOS_NOTICIA`(IN p_noticia_id INT)
BEGIN
    SELECT 
        c.id,
        c.contenido,
        c.fecha_creacion,
        c.estado,
        u.nombre,
        u.apellido_paterno,
        u.apellido_materno
    FROM comentarios_noticia c
    INNER JOIN usuarios u ON c.usuario_rut = u.rut
    WHERE c.noticia_id = p_noticia_id 
    AND c.estado = 'ACTIVO'
    ORDER BY c.fecha_creacion DESC;
END ;;
DELIMITER ;

-- SP para crear comentario en noticia
DELIMITER ;;
CREATE PROCEDURE `SP_CREAR_COMENTARIO_NOTICIA`(
    IN p_noticia_id INT,
    IN p_usuario_rut INT,
    IN p_contenido VARCHAR(500)
)
BEGIN
    DECLARE v_usuario_nombre VARCHAR(100);
    
    -- Obtener nombre del usuario
    SELECT CONCAT(nombre, ' ', apellido_paterno) INTO v_usuario_nombre
    FROM usuarios 
    WHERE rut = p_usuario_rut;
    
    -- Insertar comentario
    INSERT INTO comentarios_noticia (noticia_id, usuario_rut, usuario_nombre, contenido)
    VALUES (p_noticia_id, p_usuario_rut, v_usuario_nombre, p_contenido);
    
    SELECT LAST_INSERT_ID() as comentario_id;
END ;;
DELIMITER ;

-- =====================================================
-- 6. VERIFICACIÓN FINAL
-- =====================================================

-- Verificar que las correcciones se aplicaron correctamente
SELECT 'Verificación de tablas corregidas:' as mensaje;

-- Verificar tabla noticias
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME IN ('alcance', 'prioridad', 'estado', 'categoria', 'tags', 'autor_nombre', 'autor_rut');

-- Verificar tabla notificaciones
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME IN ('fecha_envio', 'destinatarios', 'noticia_id', 'prioridad', 'metadata');

-- Verificar tabla comentarios_noticia
SELECT COUNT(*) as total_comentarios FROM comentarios_noticia;

-- Verificar tabla dispositivos_usuario
SELECT COUNT(*) as total_dispositivos FROM dispositivos_usuario;

SELECT 'Correcciones aplicadas exitosamente!' as resultado; 