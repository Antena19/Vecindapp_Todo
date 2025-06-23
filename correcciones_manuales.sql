-- =====================================================
-- CORRECCIONES MANUALES - EJECUTAR UNO POR UNO
-- =====================================================

USE `vecindapp_bd`;

-- =====================================================
-- PASO 1: VERIFICAR QUÉ TIENES ACTUALMENTE
-- =====================================================

-- Ejecutar esto primero para ver qué columnas ya tienes:
SELECT 'COLUMNAS EN TABLA NOTICIAS:' as mensaje;
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias'
ORDER BY ORDINAL_POSITION;

SELECT 'COLUMNAS EN TABLA NOTIFICACIONES:' as mensaje;
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones'
ORDER BY ORDINAL_POSITION;

-- =====================================================
-- PASO 2: AGREGAR COLUMNAS A NOTICIAS (EJECUTAR SOLO LAS QUE FALTEN)
-- =====================================================

-- Si no tienes 'prioridad' en noticias, ejecutar:
-- ALTER TABLE `noticias` ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA';

-- Si no tienes 'estado' en noticias, ejecutar:
-- ALTER TABLE `noticias` ADD COLUMN `estado` VARCHAR(20) NOT NULL DEFAULT 'ACTIVO' COMMENT 'ACTIVO, INACTIVO';

-- Si no tienes 'categoria' en noticias, ejecutar:
-- ALTER TABLE `noticias` ADD COLUMN `categoria` VARCHAR(20) NOT NULL DEFAULT 'NOTICIA' COMMENT 'NOTICIA, EVENTO, AVISO';

-- Si no tienes 'tags' en noticias, ejecutar:
-- ALTER TABLE `noticias` ADD COLUMN `tags` TEXT NULL COMMENT 'JSON array como string';

-- Si no tienes 'autor_nombre' en noticias, ejecutar:
-- ALTER TABLE `noticias` ADD COLUMN `autor_nombre` VARCHAR(100) NULL;

-- =====================================================
-- PASO 3: AGREGAR COLUMNAS A NOTIFICACIONES (EJECUTAR SOLO LAS QUE FALTEN)
-- =====================================================

-- Si no tienes 'fecha_envio' en notificaciones, ejecutar:
-- ALTER TABLE `notificaciones` ADD COLUMN `fecha_envio` DATETIME NULL;

-- Si no tienes 'destinatarios' en notificaciones, ejecutar:
-- ALTER TABLE `notificaciones` ADD COLUMN `destinatarios` TEXT NULL COMMENT 'JSON array de RUTs como string';

-- Si no tienes 'noticia_id' en notificaciones, ejecutar:
-- ALTER TABLE `notificaciones` ADD COLUMN `noticia_id` INT NULL;

-- Si no tienes 'prioridad' en notificaciones, ejecutar:
-- ALTER TABLE `notificaciones` ADD COLUMN `prioridad` VARCHAR(20) NOT NULL DEFAULT 'MEDIA' COMMENT 'BAJA, MEDIA, ALTA';

-- Si no tienes 'metadata' en notificaciones, ejecutar:
-- ALTER TABLE `notificaciones` ADD COLUMN `metadata` TEXT NULL COMMENT 'JSON object como string';

-- =====================================================
-- PASO 4: CREAR TABLAS NUEVAS (SI NO EXISTEN)
-- =====================================================

-- Crear tabla comentarios_noticia (ejecutar solo si no existe):
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

-- Crear tabla dispositivos_usuario (ejecutar solo si no existe):
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

-- =====================================================
-- PASO 5: VERIFICACIÓN FINAL
-- =====================================================

-- Ejecutar esto al final para verificar:
SELECT 'VERIFICACIÓN FINAL:' as mensaje;
SELECT 'Tablas creadas:' as tipo;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME IN ('comentarios_noticia', 'dispositivos_usuario');

SELECT 'Tu base de datos está lista para el backend!' as resultado; 