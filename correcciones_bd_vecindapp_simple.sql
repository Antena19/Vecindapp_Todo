-- =====================================================
-- CORRECCIONES BASE DE DATOS VECINDAPP - VERSIÓN SIMPLE
-- =====================================================

USE `vecindapp_bd`;

-- =====================================================
-- 1. CORRECCIÓN TABLA NOTICIAS
-- =====================================================

-- Agregar columnas faltantes una por una (con manejo de errores)
-- Columna alcance
ALTER TABLE `noticias` 
ADD COLUMN `alcance` VARCHAR(20) NOT NULL DEFAULT 'PUBLICO' COMMENT 'PUBLICO, SOCIOS';

-- Columna prioridad
ALTER TABLE `noticias` 
ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA';

-- Columna estado
ALTER TABLE `noticias` 
ADD COLUMN `estado` VARCHAR(20) NOT NULL DEFAULT 'ACTIVO' COMMENT 'ACTIVO, INACTIVO';

-- Columna categoria
ALTER TABLE `noticias` 
ADD COLUMN `categoria` VARCHAR(20) NOT NULL DEFAULT 'NOTICIA' COMMENT 'NOTICIA, EVENTO, AVISO';

-- Columna tags
ALTER TABLE `noticias` 
ADD COLUMN `tags` TEXT NULL COMMENT 'JSON array como string';

-- Columna autor_nombre
ALTER TABLE `noticias` 
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
WHERE `id` > 0;

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

-- Agregar columnas faltantes una por una
-- Columna fecha_envio
ALTER TABLE `notificaciones` 
ADD COLUMN `fecha_envio` DATETIME NULL;

-- Columna destinatarios
ALTER TABLE `notificaciones` 
ADD COLUMN `destinatarios` TEXT NULL COMMENT 'JSON array de RUTs como string';

-- Columna noticia_id
ALTER TABLE `notificaciones` 
ADD COLUMN `noticia_id` INT NULL;

-- Columna prioridad
ALTER TABLE `notificaciones` 
ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA';

-- Columna metadata
ALTER TABLE `notificaciones` 
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
-- 4. CREAR TABLA DISPOSITIVOS_USUARIO
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