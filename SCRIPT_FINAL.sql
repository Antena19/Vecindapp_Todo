-- =====================================================
-- CORRECCIONES FINALES Y ESPECÍFICAS
-- =====================================================

USE `vecindapp_bd`;

-- =====================================================
-- PASO 1: CORREGIR TABLA NOTICIAS
-- =====================================================

-- Renombrar 'publicado_por' a 'autor_rut' para que coincida con el backend
ALTER TABLE `noticias` 
CHANGE COLUMN `publicado_por` `autor_rut` INT NOT NULL;

SELECT 'Tabla NOTICIAS corregida.' as resultado;

-- =====================================================
-- PASO 2: CORREGIR TABLA NOTIFICACIONES
-- =====================================================

-- Agregar las columnas que faltan en la tabla notificaciones
ALTER TABLE `notificaciones` 
ADD COLUMN `fecha_envio` DATETIME NULL,
ADD COLUMN `destinatarios` TEXT NULL COMMENT 'JSON array de RUTs como string',
ADD COLUMN `noticia_id` INT NULL,
ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA',
ADD COLUMN `metadata` TEXT NULL COMMENT 'JSON object como string';

-- Agregar la relación (foreign key) que falta
ALTER TABLE `notificaciones` 
ADD CONSTRAINT `fk_notificaciones_noticia` 
FOREIGN KEY (`noticia_id`) REFERENCES `noticias` (`id`) ON DELETE SET NULL;

SELECT 'Tabla NOTIFICACIONES corregida.' as resultado;

-- =====================================================
-- PASO 3: CREAR TABLAS NUEVAS
-- =====================================================

-- Este comando es seguro, solo crea la tabla si no existe
CREATE TABLE IF NOT EXISTS `comentarios_noticia` (
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

SELECT 'Tabla COMENTARIOS_NOTICIA creada o ya existente.' as resultado;

-- Este comando también es seguro
CREATE TABLE IF NOT EXISTS `dispositivos_usuario` (
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

SELECT 'Tabla DISPOSITIVOS_USUARIO creada o ya existente.' as resultado;


-- =====================================================
-- PASO 4: CREAR/ACTUALIZAR STORED PROCEDURES
-- =====================================================

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

DROP PROCEDURE IF EXISTS `SP_CREAR_COMENTARIO_NOTICIA`;
DELIMITER ;;
CREATE PROCEDURE `SP_CREAR_COMENTARIO_NOTICIA`(
    IN p_noticia_id INT,
    IN p_usuario_rut INT,
    IN p_contenido VARCHAR(500)
)
BEGIN
    DECLARE v_usuario_nombre VARCHAR(100);
    
    SELECT CONCAT(nombre, ' ', apellido_paterno) INTO v_usuario_nombre
    FROM usuarios 
    WHERE rut = p_usuario_rut;
    
    INSERT INTO comentarios_noticia (noticia_id, usuario_rut, usuario_nombre, contenido)
    VALUES (p_noticia_id, p_usuario_rut, v_usuario_nombre, p_contenido);
    
    SELECT LAST_INSERT_ID() as comentario_id;
END ;;
DELIMITER ;

SELECT 'Stored Procedures creados/actualizados.' as resultado;

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================

SELECT '¡BASE DE DATOS LISTA!' as ESTADO;
SELECT 'Ahora puedes ejecutar el backend.' as SIGUIENTE_PASO; 