-- =====================================================
-- CORRECCIONES BASE DE DATOS VECINDAPP - VERSIÓN FINAL
-- =====================================================

USE `vecindapp_bd`;

-- =====================================================
-- 1. VERIFICAR QUÉ COLUMNAS YA EXISTEN
-- =====================================================

SELECT 'VERIFICANDO COLUMNAS EXISTENTES...' as mensaje;

-- Verificar columnas en tabla noticias
SELECT 'Columnas existentes en tabla noticias:' as tabla;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias'
ORDER BY ORDINAL_POSITION;

-- Verificar columnas en tabla notificaciones
SELECT 'Columnas existentes en tabla notificaciones:' as tabla;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones'
ORDER BY ORDINAL_POSITION;

-- =====================================================
-- 2. AGREGAR SOLO LAS COLUMNAS QUE FALTAN EN NOTICIAS
-- =====================================================

-- Verificar si existe 'prioridad' en noticias
SELECT COUNT(*) INTO @existe_prioridad
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'prioridad';

-- Agregar prioridad si no existe
SET @sql = '';
IF @existe_prioridad = 0 THEN
    ALTER TABLE `noticias` ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA';
    SELECT 'Columna prioridad agregada a noticias' as resultado;
ELSE
    SELECT 'Columna prioridad ya existe en noticias' as resultado;
END IF;

-- Verificar si existe 'estado' en noticias
SELECT COUNT(*) INTO @existe_estado
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'estado';

-- Agregar estado si no existe
IF @existe_estado = 0 THEN
    ALTER TABLE `noticias` ADD COLUMN `estado` VARCHAR(20) NOT NULL DEFAULT 'ACTIVO' COMMENT 'ACTIVO, INACTIVO';
    SELECT 'Columna estado agregada a noticias' as resultado;
ELSE
    SELECT 'Columna estado ya existe en noticias' as resultado;
END IF;

-- Verificar si existe 'categoria' en noticias
SELECT COUNT(*) INTO @existe_categoria
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'categoria';

-- Agregar categoria si no existe
IF @existe_categoria = 0 THEN
    ALTER TABLE `noticias` ADD COLUMN `categoria` VARCHAR(20) NOT NULL DEFAULT 'NOTICIA' COMMENT 'NOTICIA, EVENTO, AVISO';
    SELECT 'Columna categoria agregada a noticias' as resultado;
ELSE
    SELECT 'Columna categoria ya existe en noticias' as resultado;
END IF;

-- Verificar si existe 'tags' en noticias
SELECT COUNT(*) INTO @existe_tags
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'tags';

-- Agregar tags si no existe
IF @existe_tags = 0 THEN
    ALTER TABLE `noticias` ADD COLUMN `tags` TEXT NULL COMMENT 'JSON array como string';
    SELECT 'Columna tags agregada a noticias' as resultado;
ELSE
    SELECT 'Columna tags ya existe en noticias' as resultado;
END IF;

-- Verificar si existe 'autor_nombre' en noticias
SELECT COUNT(*) INTO @existe_autor_nombre
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias' 
AND COLUMN_NAME = 'autor_nombre';

-- Agregar autor_nombre si no existe
IF @existe_autor_nombre = 0 THEN
    ALTER TABLE `noticias` ADD COLUMN `autor_nombre` VARCHAR(100) NULL;
    SELECT 'Columna autor_nombre agregada a noticias' as resultado;
ELSE
    SELECT 'Columna autor_nombre ya existe en noticias' as resultado;
END IF;

-- =====================================================
-- 3. AGREGAR SOLO LAS COLUMNAS QUE FALTAN EN NOTIFICACIONES
-- =====================================================

-- Verificar si existe 'fecha_envio' en notificaciones
SELECT COUNT(*) INTO @existe_fecha_envio
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'fecha_envio';

-- Agregar fecha_envio si no existe
IF @existe_fecha_envio = 0 THEN
    ALTER TABLE `notificaciones` ADD COLUMN `fecha_envio` DATETIME NULL;
    SELECT 'Columna fecha_envio agregada a notificaciones' as resultado;
ELSE
    SELECT 'Columna fecha_envio ya existe en notificaciones' as resultado;
END IF;

-- Verificar si existe 'destinatarios' en notificaciones
SELECT COUNT(*) INTO @existe_destinatarios
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'destinatarios';

-- Agregar destinatarios si no existe
IF @existe_destinatarios = 0 THEN
    ALTER TABLE `notificaciones` ADD COLUMN `destinatarios` TEXT NULL COMMENT 'JSON array de RUTs como string';
    SELECT 'Columna destinatarios agregada a notificaciones' as resultado;
ELSE
    SELECT 'Columna destinatarios ya existe en notificaciones' as resultado;
END IF;

-- Verificar si existe 'noticia_id' en notificaciones
SELECT COUNT(*) INTO @existe_noticia_id
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'noticia_id';

-- Agregar noticia_id si no existe
IF @existe_noticia_id = 0 THEN
    ALTER TABLE `notificaciones` ADD COLUMN `noticia_id` INT NULL;
    SELECT 'Columna noticia_id agregada a notificaciones' as resultado;
ELSE
    SELECT 'Columna noticia_id ya existe en notificaciones' as resultado;
END IF;

-- Verificar si existe 'prioridad' en notificaciones
SELECT COUNT(*) INTO @existe_prioridad_notif
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'prioridad';

-- Agregar prioridad si no existe
IF @existe_prioridad_notif = 0 THEN
    ALTER TABLE `notificaciones` ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA';
    SELECT 'Columna prioridad agregada a notificaciones' as resultado;
ELSE
    SELECT 'Columna prioridad ya existe en notificaciones' as resultado;
END IF;

-- Verificar si existe 'metadata' en notificaciones
SELECT COUNT(*) INTO @existe_metadata
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones' 
AND COLUMN_NAME = 'metadata';

-- Agregar metadata si no existe
IF @existe_metadata = 0 THEN
    ALTER TABLE `notificaciones` ADD COLUMN `metadata` TEXT NULL COMMENT 'JSON object como string';
    SELECT 'Columna metadata agregada a notificaciones' as resultado;
ELSE
    SELECT 'Columna metadata ya existe en notificaciones' as resultado;
END IF;

-- =====================================================
-- 4. CREAR TABLAS NUEVAS (SI NO EXISTEN)
-- =====================================================

-- Verificar si existe la tabla comentarios_noticia
SELECT COUNT(*) INTO @existe_comentarios_noticia
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'comentarios_noticia';

-- Crear tabla comentarios_noticia si no existe
IF @existe_comentarios_noticia = 0 THEN
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

-- Verificar si existe la tabla dispositivos_usuario
SELECT COUNT(*) INTO @existe_dispositivos_usuario
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'dispositivos_usuario';

-- Crear tabla dispositivos_usuario si no existe
IF @existe_dispositivos_usuario = 0 THEN
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
-- 5. VERIFICACIÓN FINAL
-- =====================================================

SELECT 'VERIFICACIÓN FINAL COMPLETADA!' as mensaje;
SELECT 'Tu base de datos está lista para el backend.' as resultado; 