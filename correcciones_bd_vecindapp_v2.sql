-- =====================================================
-- CORRECCIONES BASE DE DATOS VECINDAPP - VERSIÓN 2
-- =====================================================

USE `vecindapp_bd`;

-- =====================================================
-- 1. CORRECCIÓN TABLA NOTICIAS (CON VERIFICACIÓN)
-- =====================================================

-- Verificar y agregar columnas faltantes a la tabla noticias
SET @sql = '';

-- Verificar si existe la columna 'alcance'
SELECT COUNT(*) INTO @existe_alcance
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'alcance';

IF @existe_alcance = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `alcance` VARCHAR(20) NOT NULL DEFAULT ''PUBLICO'' COMMENT ''PUBLICO, SOCIOS'',');
END IF;

-- Verificar si existe la columna 'prioridad'
SELECT COUNT(*) INTO @existe_prioridad
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'prioridad';

IF @existe_prioridad = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT ''MEDIA'' COMMENT ''BAJA, MEDIA, ALTA'',');
END IF;

-- Verificar si existe la columna 'estado'
SELECT COUNT(*) INTO @existe_estado
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'estado';

IF @existe_estado = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `estado` VARCHAR(20) NOT NULL DEFAULT ''ACTIVO'' COMMENT ''ACTIVO, INACTIVO'',');
END IF;

-- Verificar si existe la columna 'categoria'
SELECT COUNT(*) INTO @existe_categoria
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'categoria';

IF @existe_categoria = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `categoria` VARCHAR(20) NOT NULL DEFAULT ''NOTICIA'' COMMENT ''NOTICIA, EVENTO, AVISO'',');
END IF;

-- Verificar si existe la columna 'tags'
SELECT COUNT(*) INTO @existe_tags
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'tags';

IF @existe_tags = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `tags` TEXT NULL COMMENT ''JSON array como string'',');
END IF;

-- Verificar si existe la columna 'autor_nombre'
SELECT COUNT(*) INTO @existe_autor_nombre
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'autor_nombre';

IF @existe_autor_nombre = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `autor_nombre` VARCHAR(100) NULL,');
END IF;

-- Ejecutar ALTER TABLE solo si hay columnas para agregar
IF LENGTH(@sql) > 0 THEN
    SET @sql = CONCAT('ALTER TABLE `noticias` ', TRIM(TRAILING ',' FROM @sql));
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
    SELECT 'Columnas agregadas a tabla noticias' as resultado;
ELSE
    SELECT 'Todas las columnas ya existen en tabla noticias' as resultado;
END IF;

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

-- Verificar si existe la columna 'autor_rut' (antes era 'publicado_por')
SELECT COUNT(*) INTO @existe_autor_rut
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'autor_rut';

-- Verificar si existe la columna 'publicado_por'
SELECT COUNT(*) INTO @existe_publicado_por
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'publicado_por';

-- Renombrar campos para coincidir con el modelo solo si es necesario
IF @existe_autor_rut = 0 AND @existe_publicado_por > 0 THEN
    ALTER TABLE `noticias` CHANGE COLUMN `publicado_por` `autor_rut` INT NOT NULL;
    SELECT 'Columna publicado_por renombrada a autor_rut' as resultado;
ELSE
    SELECT 'Columna autor_rut ya existe o publicado_por no existe' as resultado;
END IF;

-- Crear índices solo si no existen
CREATE INDEX IF NOT EXISTS `idx_noticias_estado` ON `noticias` (`estado`);
CREATE INDEX IF NOT EXISTS `idx_noticias_categoria` ON `noticias` (`categoria`);
CREATE INDEX IF NOT EXISTS `idx_noticias_alcance` ON `noticias` (`alcance`);

-- =====================================================
-- 2. CORRECCIÓN TABLA NOTIFICACIONES (CON VERIFICACIÓN)
-- =====================================================

-- Verificar y agregar columnas faltantes a la tabla notificaciones
SET @sql = '';

-- Verificar si existe la columna 'fecha_envio'
SELECT COUNT(*) INTO @existe_fecha_envio
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'fecha_envio';

IF @existe_fecha_envio = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `fecha_envio` DATETIME NULL,');
END IF;

-- Verificar si existe la columna 'destinatarios'
SELECT COUNT(*) INTO @existe_destinatarios
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'destinatarios';

IF @existe_destinatarios = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `destinatarios` TEXT NULL COMMENT ''JSON array de RUTs como string'',');
END IF;

-- Verificar si existe la columna 'noticia_id'
SELECT COUNT(*) INTO @existe_noticia_id
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'noticia_id';

IF @existe_noticia_id = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `noticia_id` INT NULL,');
END IF;

-- Verificar si existe la columna 'prioridad'
SELECT COUNT(*) INTO @existe_prioridad_notif
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'prioridad';

IF @existe_prioridad_notif = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT ''MEDIA'' COMMENT ''BAJA, MEDIA, ALTA'',');
END IF;

-- Verificar si existe la columna 'metadata'
SELECT COUNT(*) INTO @existe_metadata
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'metadata';

IF @existe_metadata = 0 THEN
    SET @sql = CONCAT(@sql, 'ADD COLUMN `metadata` TEXT NULL COMMENT ''JSON object como string'',');
END IF;

-- Ejecutar ALTER TABLE solo si hay columnas para agregar
IF LENGTH(@sql) > 0 THEN
    SET @sql = CONCAT('ALTER TABLE `notificaciones` ', TRIM(TRAILING ',' FROM @sql));
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
    SELECT 'Columnas agregadas a tabla notificaciones' as resultado;
ELSE
    SELECT 'Todas las columnas ya existen en tabla notificaciones' as resultado;
END IF;

-- Agregar foreign key para noticia_id solo si no existe
SELECT COUNT(*) INTO @existe_fk_notificaciones_noticia
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND CONSTRAINT_NAME = 'fk_notificaciones_noticia';

IF @existe_fk_notificaciones_noticia = 0 THEN
    ALTER TABLE `notificaciones` 
    ADD CONSTRAINT `fk_notificaciones_noticia` 
    FOREIGN KEY (`noticia_id`) REFERENCES `noticias` (`id`) ON DELETE SET NULL;
    SELECT 'Foreign key agregada a notificaciones' as resultado;
ELSE
    SELECT 'Foreign key ya existe en notificaciones' as resultado;
END IF;

-- Crear índices solo si no existen
CREATE INDEX IF NOT EXISTS `idx_notificaciones_prioridad` ON `notificaciones` (`prioridad`);
CREATE INDEX IF NOT EXISTS `idx_notificaciones_noticia_id` ON `notificaciones` (`noticia_id`);

-- =====================================================
-- 3. CREAR TABLA COMENTARIOS_NOTICIA (SI NO EXISTE)
-- =====================================================

-- Verificar si la tabla comentarios_noticia existe
SELECT COUNT(*) INTO @existe_comentarios_noticia
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'comentarios_noticia';

IF @existe_comentarios_noticia = 0 THEN
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
    SELECT 'Tabla comentarios_noticia creada' as resultado;
ELSE
    SELECT 'Tabla comentarios_noticia ya existe' as resultado;
END IF;

-- =====================================================
-- 4. CREAR TABLA DISPOSITIVOS_USUARIO (SI NO EXISTE)
-- =====================================================

-- Verificar si la tabla dispositivos_usuario existe
SELECT COUNT(*) INTO @existe_dispositivos_usuario
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'dispositivos_usuario';

IF @existe_dispositivos_usuario = 0 THEN
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
    SELECT 'Tabla dispositivos_usuario creada' as resultado;
ELSE
    SELECT 'Tabla dispositivos_usuario ya existe' as resultado;
END IF;

-- =====================================================
-- 5. STORED PROCEDURES ADICIONALES
-- =====================================================

-- SP para obtener estadísticas de noticias
DROP PROCEDURE IF EXISTS `SP_OBTENER_ESTADISTICAS_NOTICIAS`;
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
DROP PROCEDURE IF EXISTS `SP_OBTENER_COMENTARIOS_NOTICIA`;
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
DROP PROCEDURE IF EXISTS `SP_CREAR_COMENTARIO_NOTICIA`;
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

SELECT 'VERIFICACIÓN FINAL DE CORRECCIONES:' as mensaje;

-- Verificar tabla noticias
SELECT 'Columnas en tabla noticias:' as tabla;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME IN ('alcance', 'prioridad', 'estado', 'categoria', 'tags', 'autor_nombre', 'autor_rut')
ORDER BY COLUMN_NAME;

-- Verificar tabla notificaciones
SELECT 'Columnas en tabla notificaciones:' as tabla;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME IN ('fecha_envio', 'destinatarios', 'noticia_id', 'prioridad', 'metadata')
ORDER BY COLUMN_NAME;

-- Verificar tablas nuevas
SELECT 'Tablas nuevas creadas:' as tabla;
SELECT 
    TABLE_NAME,
    'EXISTE' as estado
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME IN ('comentarios_noticia', 'dispositivos_usuario')
ORDER BY TABLE_NAME;

SELECT 'Todas las correcciones aplicadas exitosamente!' as resultado; 