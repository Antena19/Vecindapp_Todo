-- =====================================================
-- VERIFICACIÓN DE COLUMNAS EXISTENTES
-- =====================================================

USE `vecindapp_bd`;

-- Verificar columnas existentes en tabla noticias
SELECT 'COLUMNAS EXISTENTES EN NOTICIAS:' as mensaje;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'noticias'
ORDER BY ORDINAL_POSITION;

-- Verificar columnas existentes en tabla notificaciones
SELECT 'COLUMNAS EXISTENTES EN NOTIFICACIONES:' as mensaje;
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME = 'notificaciones'
ORDER BY ORDINAL_POSITION;

-- Verificar si las tablas nuevas ya existen
SELECT 'TABLAS NUEVAS:' as mensaje;
SELECT 
    TABLE_NAME,
    CASE 
        WHEN TABLE_NAME = 'comentarios_noticia' THEN 'EXISTE'
        WHEN TABLE_NAME = 'dispositivos_usuario' THEN 'EXISTE'
        ELSE 'NO EXISTE'
    END as estado
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'vecindapp_bd' 
AND TABLE_NAME IN ('comentarios_noticia', 'dispositivos_usuario'); 