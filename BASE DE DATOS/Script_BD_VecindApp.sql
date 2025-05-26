CREATE DATABASE  IF NOT EXISTS `vecindapp_bd` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `vecindapp_bd`;
-- MySQL dump 10.13  Distrib 8.0.41, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: vecindapp_bd
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `asistencia_eventos`
--

DROP TABLE IF EXISTS `asistencia_eventos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `asistencia_eventos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `evento_id` int NOT NULL,
  `usuario_rut` int NOT NULL,
  `fecha_asistencia` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `evento_usuario_UNIQUE` (`evento_id`,`usuario_rut`),
  KEY `evento_id_idx` (`evento_id`),
  KEY `usuario_asistencia_rut_idx` (`usuario_rut`),
  CONSTRAINT `evento_id_fk` FOREIGN KEY (`evento_id`) REFERENCES `eventos` (`id`) ON DELETE CASCADE,
  CONSTRAINT `usuario_asistencia_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `asistencia_eventos`
--

LOCK TABLES `asistencia_eventos` WRITE;
/*!40000 ALTER TABLE `asistencia_eventos` DISABLE KEYS */;
/*!40000 ALTER TABLE `asistencia_eventos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `certificados`
--

DROP TABLE IF EXISTS `certificados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `certificados` (
  `id` int NOT NULL AUTO_INCREMENT,
  `solicitud_id` int NOT NULL,
  `codigo_verificacion` varchar(50) NOT NULL,
  `fecha_emision` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_vencimiento` date DEFAULT NULL,
  `archivo_pdf` varchar(255) NOT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'vigente',
  PRIMARY KEY (`id`),
  UNIQUE KEY `codigo_verificacion_UNIQUE` (`codigo_verificacion`),
  KEY `solicitud_id_idx` (`solicitud_id`),
  CONSTRAINT `solicitud_id_fk` FOREIGN KEY (`solicitud_id`) REFERENCES `solicitudes_certificado` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `certificados`
--

LOCK TABLES `certificados` WRITE;
/*!40000 ALTER TABLE `certificados` DISABLE KEYS */;
INSERT INTO `certificados` VALUES (1,1,'CERT-952959','2025-05-23 19:06:22','2025-08-23','/certificados/CERT-952959.pdf','vigente');
/*!40000 ALTER TABLE `certificados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `configuraciones`
--

DROP TABLE IF EXISTS `configuraciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configuraciones` (
  `id` int NOT NULL AUTO_INCREMENT,
  `clave` varchar(100) NOT NULL,
  `valor` text NOT NULL,
  `descripcion` text,
  `modificado_por` int DEFAULT NULL,
  `fecha_modificacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `clave_UNIQUE` (`clave`),
  KEY `modificado_por_idx` (`modificado_por`),
  CONSTRAINT `modificado_por_fk` FOREIGN KEY (`modificado_por`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configuraciones`
--

LOCK TABLES `configuraciones` WRITE;
/*!40000 ALTER TABLE `configuraciones` DISABLE KEYS */;
INSERT INTO `configuraciones` VALUES (1,'nombre_jjvv','Junta de Vecinos Portal Puerto Montt','Nombre oficial de la junta de vecinos',17144575,'2025-04-08 16:08:10'),(2,'direccion_sede','Calle Principal 456, Puerto Montt','Dirección de la sede vecinal',17144575,'2025-04-08 16:08:10'),(3,'telefono_contacto','+56912345678','Teléfono de contacto oficial',17144575,'2025-04-08 16:08:10'),(4,'email_contacto','jjvvportalpuertomontt@gmail.com','Correo electrónico oficial',17144575,'2025-04-08 16:08:10');
/*!40000 ALTER TABLE `configuraciones` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `cuotas_socio`
--

DROP TABLE IF EXISTS `cuotas_socio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cuotas_socio` (
  `id` int NOT NULL AUTO_INCREMENT,
  `idsocio` int NOT NULL,
  `tipo_cuota_id` int NOT NULL,
  `fecha_generacion` date NOT NULL,
  `fecha_vencimiento` date NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'pendiente',
  `pago_id` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idsocio_idx` (`idsocio`),
  KEY `tipo_cuota_idx` (`tipo_cuota_id`),
  KEY `pago_id_idx` (`pago_id`),
  CONSTRAINT `pago_id_fk` FOREIGN KEY (`pago_id`) REFERENCES `pagos` (`id`),
  CONSTRAINT `tipo_cuota_fk` FOREIGN KEY (`tipo_cuota_id`) REFERENCES `tipos_cuota` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `cuotas_socio`
--

LOCK TABLES `cuotas_socio` WRITE;
/*!40000 ALTER TABLE `cuotas_socio` DISABLE KEYS */;
/*!40000 ALTER TABLE `cuotas_socio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dispositivos_usuario`
--

DROP TABLE IF EXISTS `dispositivos_usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dispositivos_usuario` (
  `id` int NOT NULL AUTO_INCREMENT,
  `usuario_rut` int NOT NULL,
  `token` varchar(255) NOT NULL,
  `plataforma` varchar(20) NOT NULL,
  `modelo` varchar(100) DEFAULT NULL,
  `ultima_conexion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `activo` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `usuario_disp_rut_idx` (`usuario_rut`),
  CONSTRAINT `usuario_disp_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dispositivos_usuario`
--

LOCK TABLES `dispositivos_usuario` WRITE;
/*!40000 ALTER TABLE `dispositivos_usuario` DISABLE KEYS */;
/*!40000 ALTER TABLE `dispositivos_usuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `eventos`
--

DROP TABLE IF EXISTS `eventos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `eventos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `titulo` varchar(200) NOT NULL,
  `descripcion` text NOT NULL,
  `fecha_evento` date NOT NULL,
  `hora_evento` time NOT NULL,
  `lugar` varchar(255) NOT NULL,
  `directiva_rut` int NOT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'activo',
  `codigo_qr` varchar(255) NOT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `notas` text,
  PRIMARY KEY (`id`),
  KEY `directiva_evento_rut_idx` (`directiva_rut`),
  CONSTRAINT `directiva_evento_rut_fk` FOREIGN KEY (`directiva_rut`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `eventos`
--

LOCK TABLES `eventos` WRITE;
/*!40000 ALTER TABLE `eventos` DISABLE KEYS */;
INSERT INTO `eventos` VALUES (3,'Asamblea General Extraordinaria','Reunión para discutir temas importantes de la comunidad y elección de nueva directiva.','2024-03-25','19:00:00','Salón Comunitario Portal Puerto Montt',17144575,'activo','QR_ASAMBLEA_2024_03_25','2025-05-26 13:05:05','Se requiere asistencia mínima del 50% de los socios para quórum.'),(4,'Jornada de Limpieza Comunitaria','Actividad de limpieza y mantenimiento de áreas comunes. Se recomienda traer guantes y bolsas.','2024-03-30','09:00:00','Áreas Verdes del Condominio',17144575,'activo','QR_LIMPIEZA_2024_03_30','2025-05-26 13:05:05','Se proporcionarán herramientas y refrigerios a los participantes.');
/*!40000 ALTER TABLE `eventos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `lecturas_noticia`
--

DROP TABLE IF EXISTS `lecturas_noticia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `lecturas_noticia` (
  `id` int NOT NULL AUTO_INCREMENT,
  `noticia_id` int NOT NULL,
  `usuario_rut` int NOT NULL,
  `fecha_lectura` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `noticia_usuario_UNIQUE` (`noticia_id`,`usuario_rut`),
  KEY `noticia_id_idx` (`noticia_id`),
  KEY `usuario_lectura_rut_idx` (`usuario_rut`),
  CONSTRAINT `noticia_id_fk` FOREIGN KEY (`noticia_id`) REFERENCES `noticias` (`id`) ON DELETE CASCADE,
  CONSTRAINT `usuario_lectura_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `lecturas_noticia`
--

LOCK TABLES `lecturas_noticia` WRITE;
/*!40000 ALTER TABLE `lecturas_noticia` DISABLE KEYS */;
/*!40000 ALTER TABLE `lecturas_noticia` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `noticias`
--

DROP TABLE IF EXISTS `noticias`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `noticias` (
  `id` int NOT NULL AUTO_INCREMENT,
  `titulo` varchar(200) NOT NULL,
  `contenido` text NOT NULL,
  `fecha_publicacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_evento` datetime DEFAULT NULL,
  `lugar` varchar(255) DEFAULT NULL,
  `imagen` varchar(255) DEFAULT NULL,
  `publicado_por` int NOT NULL,
  `visibilidad` varchar(20) NOT NULL DEFAULT 'todos',
  `destacado` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `publicado_por_idx` (`publicado_por`),
  CONSTRAINT `publicado_por_fk` FOREIGN KEY (`publicado_por`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `noticias`
--

LOCK TABLES `noticias` WRITE;
/*!40000 ALTER TABLE `noticias` DISABLE KEYS */;
INSERT INTO `noticias` VALUES (1,'Reunión Mensual de Vecinos','Invitamos a todos los vecinos a participar en nuestra reunión mensual donde trataremos temas importantes para nuestra comunidad.','2025-04-08 00:00:00','2025-04-18 00:00:00','Sede Vecinal Portal Puerto Montt',NULL,17144575,'todos',1),(2,'Actualización de Cuotas','Informamos que las cuotas se mantienen en $1.000 pesos para este año. Recuerde mantenerse al día para acceder a los beneficios.','2025-04-08 00:00:00',NULL,NULL,NULL,17144575,'solo_socios',0);
/*!40000 ALTER TABLE `noticias` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notificaciones`
--

DROP TABLE IF EXISTS `notificaciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notificaciones` (
  `id` int NOT NULL AUTO_INCREMENT,
  `usuario_rut` int NOT NULL,
  `titulo` varchar(100) NOT NULL,
  `mensaje` text NOT NULL,
  `tipo` varchar(20) NOT NULL DEFAULT 'info',
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `leida` tinyint NOT NULL DEFAULT '0',
  `fecha_lectura` datetime DEFAULT NULL,
  `referencia_tipo` varchar(50) DEFAULT NULL,
  `referencia_id` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `usuario_notif_rut_idx` (`usuario_rut`),
  KEY `leida_idx` (`leida`),
  CONSTRAINT `usuario_notif_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notificaciones`
--

LOCK TABLES `notificaciones` WRITE;
/*!40000 ALTER TABLE `notificaciones` DISABLE KEYS */;
/*!40000 ALTER TABLE `notificaciones` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pagos`
--

DROP TABLE IF EXISTS `pagos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pagos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `usuario_rut` int NOT NULL,
  `tipo` varchar(20) NOT NULL,
  `referencia_id` int NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `fecha_pago` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `metodo_pago` varchar(20) NOT NULL,
  `comprobante` varchar(255) DEFAULT NULL,
  `estado` varchar(20) NOT NULL DEFAULT 'pendiente' COMMENT 'Estados: pendiente, procesando, aprobado, rechazado, error',
  `codigo_transaccion` varchar(100) DEFAULT NULL,
  `observaciones` text,
  `token_webpay` varchar(255) DEFAULT NULL COMMENT 'Token de transacción de WebPay',
  `url_pago_webpay` varchar(255) DEFAULT NULL COMMENT 'URL de pago de WebPay',
  `respuesta_webpay` text COMMENT 'Respuesta completa de WebPay',
  PRIMARY KEY (`id`),
  KEY `usuario_pago_rut_idx` (`usuario_rut`),
  CONSTRAINT `usuario_pago_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pagos`
--

LOCK TABLES `pagos` WRITE;
/*!40000 ALTER TABLE `pagos` DISABLE KEYS */;
/*!40000 ALTER TABLE `pagos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `registro_actividades`
--

DROP TABLE IF EXISTS `registro_actividades`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `registro_actividades` (
  `id` int NOT NULL AUTO_INCREMENT,
  `usuario_rut` int DEFAULT NULL,
  `accion` varchar(255) NOT NULL,
  `entidad` varchar(50) NOT NULL,
  `entidad_id` int DEFAULT NULL,
  `detalles` text,
  `ip` varchar(45) DEFAULT NULL,
  `fecha_hora` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `usuario_registro_rut_idx` (`usuario_rut`),
  CONSTRAINT `usuario_registro_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `registro_actividades`
--

LOCK TABLES `registro_actividades` WRITE;
/*!40000 ALTER TABLE `registro_actividades` DISABLE KEYS */;
/*!40000 ALTER TABLE `registro_actividades` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `socios`
--

DROP TABLE IF EXISTS `socios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `socios` (
  `idsocio` int NOT NULL AUTO_INCREMENT,
  `num_socio` int DEFAULT NULL,
  `rut` int NOT NULL,
  `fecha_solicitud` date NOT NULL,
  `fecha_aprobacion` date DEFAULT NULL,
  `estado_solicitud` varchar(45) NOT NULL DEFAULT 'pendiente',
  `motivo_rechazo` varchar(200) DEFAULT NULL,
  `documento_identidad` varchar(255) DEFAULT NULL,
  `documento_domicilio` varchar(255) DEFAULT NULL,
  `estado` tinyint NOT NULL DEFAULT '0' COMMENT '''1: Activo, 0: Inactivo''',
  `motivo_desactivacion` varchar(200) DEFAULT NULL,
  `fecha_desactivacion` datetime DEFAULT NULL,
  PRIMARY KEY (`idsocio`),
  UNIQUE KEY `num_socio` (`num_socio`),
  KEY `rut_idx` (`rut`),
  CONSTRAINT `rut` FOREIGN KEY (`rut`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `socios`
--

LOCK TABLES `socios` WRITE;
/*!40000 ALTER TABLE `socios` DISABLE KEYS */;
INSERT INTO `socios` VALUES (1,200,17144575,'2023-01-10','2023-01-10','aprobada',NULL,NULL,NULL,1,NULL,NULL),(2,NULL,25592802,'2025-05-23',NULL,'pendiente',NULL,'/archivos/identidad_25592802_638835996474347036.png','/archivos/domicilio_25592802_638835996474387560.png',0,NULL,NULL);
/*!40000 ALTER TABLE `socios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `solicitudes_certificado`
--

DROP TABLE IF EXISTS `solicitudes_certificado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `solicitudes_certificado` (
  `id` int NOT NULL AUTO_INCREMENT,
  `usuario_rut` int NOT NULL,
  `tipo_certificado_id` int NOT NULL,
  `fecha_solicitud` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `estado` varchar(20) NOT NULL DEFAULT 'pendiente',
  `motivo` text,
  `documentos_adjuntos` varchar(255) DEFAULT NULL,
  `fecha_aprobacion` datetime DEFAULT NULL,
  `directiva_rut` int DEFAULT NULL,
  `precio` decimal(10,2) NOT NULL,
  `observaciones` text,
  PRIMARY KEY (`id`),
  KEY `usuario_rut_idx` (`usuario_rut`),
  KEY `tipo_certificado_idx` (`tipo_certificado_id`),
  KEY `directiva_rut_idx` (`directiva_rut`),
  CONSTRAINT `directiva_cert_rut` FOREIGN KEY (`directiva_rut`) REFERENCES `usuarios` (`rut`),
  CONSTRAINT `tipo_certificado_fk` FOREIGN KEY (`tipo_certificado_id`) REFERENCES `tipos_certificado` (`id`),
  CONSTRAINT `usuario_cert_rut` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `solicitudes_certificado`
--

LOCK TABLES `solicitudes_certificado` WRITE;
/*!40000 ALTER TABLE `solicitudes_certificado` DISABLE KEYS */;
INSERT INTO `solicitudes_certificado` VALUES (1,17144575,3,'2025-05-23 18:41:41','aprobado','Necesito certificado de residencia para trámites bancarios','documento.pdf','2025-05-23 19:06:22',17144575,3000.00,'prueba');
/*!40000 ALTER TABLE `solicitudes_certificado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_certificado`
--

DROP TABLE IF EXISTS `tipos_certificado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipos_certificado` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `descripcion` text,
  `precio_socio` decimal(10,2) NOT NULL,
  `precio_vecino` decimal(10,2) NOT NULL,
  `documentos_requeridos` text,
  `activo` tinyint NOT NULL DEFAULT '1',
  `medios_pago_habilitados` set('efectivo','transferencia','webpay','otro') DEFAULT 'efectivo,transferencia',
  PRIMARY KEY (`id`),
  UNIQUE KEY `nombre_UNIQUE` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_certificado`
--

LOCK TABLES `tipos_certificado` WRITE;
/*!40000 ALTER TABLE `tipos_certificado` DISABLE KEYS */;
INSERT INTO `tipos_certificado` VALUES (3,'Certificado de Residencia','Acredita que la persona reside en el sector',2000.00,3000.00,'Copia de cuenta de servicios reciente (luz, agua, etc.)',1,'efectivo,transferencia');
/*!40000 ALTER TABLE `tipos_certificado` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_cuota`
--

DROP TABLE IF EXISTS `tipos_cuota`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipos_cuota` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `descripcion` text,
  `monto` decimal(10,2) NOT NULL,
  `periodicidad` varchar(20) NOT NULL,
  `activo` tinyint NOT NULL DEFAULT '1',
  `medios_pago_habilitados` set('efectivo','transferencia','webpay','otro') DEFAULT 'efectivo,transferencia',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_cuota`
--

LOCK TABLES `tipos_cuota` WRITE;
/*!40000 ALTER TABLE `tipos_cuota` DISABLE KEYS */;
INSERT INTO `tipos_cuota` VALUES (1,'Cuota Mensual','Cuota mensual regular para socios',1000.00,'mensual',1,'efectivo,transferencia'),(2,'Cuota Mensual','Cuota mensual regular para socios',1000.00,'mensual',1,'efectivo,transferencia');
/*!40000 ALTER TABLE `tipos_cuota` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `transacciones_webpay`
--

DROP TABLE IF EXISTS `transacciones_webpay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `transacciones_webpay` (
  `id` int NOT NULL AUTO_INCREMENT,
  `pago_id` int DEFAULT NULL,
  `token_webpay` varchar(255) NOT NULL,
  `monto` decimal(10,2) NOT NULL,
  `fecha_creacion` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_actualizacion` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `estado` enum('iniciada','pendiente','aprobada','rechazada','error') DEFAULT 'iniciada',
  `tipo_transaccion` enum('cuota','certificado') NOT NULL,
  `usuario_rut` int NOT NULL,
  `detalle` text,
  PRIMARY KEY (`id`),
  KEY `pago_id` (`pago_id`),
  KEY `usuario_rut` (`usuario_rut`),
  CONSTRAINT `transacciones_webpay_ibfk_1` FOREIGN KEY (`pago_id`) REFERENCES `pagos` (`id`),
  CONSTRAINT `transacciones_webpay_ibfk_2` FOREIGN KEY (`usuario_rut`) REFERENCES `usuarios` (`rut`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transacciones_webpay`
--

LOCK TABLES `transacciones_webpay` WRITE;
/*!40000 ALTER TABLE `transacciones_webpay` DISABLE KEYS */;
/*!40000 ALTER TABLE `transacciones_webpay` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `rut` int NOT NULL,
  `dv_rut` char(1) NOT NULL,
  `nombre` varchar(45) NOT NULL,
  `apellido_paterno` varchar(45) NOT NULL,
  `apellido_materno` varchar(45) DEFAULT NULL,
  `correo_electronico` varchar(45) DEFAULT NULL,
  `telefono` varchar(20) NOT NULL,
  `direccion` varchar(200) NOT NULL,
  `password` varchar(255) NOT NULL,
  `fecha_registro` date NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1' COMMENT '''1: Activo, 0: Inactivo'',',
  `tipo_usuario` varchar(10) NOT NULL DEFAULT 'vecino' COMMENT '''vecino, socio, directiva''',
  `token_recuperacion` varchar(255) DEFAULT NULL,
  `fecha_token_recuperacion` datetime DEFAULT NULL,
  PRIMARY KEY (`rut`),
  UNIQUE KEY `rut_UNIQUE` (`rut`),
  UNIQUE KEY `correo_electronico_UNIQUE` (`correo_electronico`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (17144575,'2','Angelina','Mendoza','Yañez','angelina.mendoza.y@gmail.com','+56998555466','Joseph Addison 2342 ','30b62cbe41ff0cd5a6cd8ed2ff4f47d4a152b56e0e79587a3758137f58d2bec8','2025-04-13',1,'directiva','cfd0d89c946588d802cde8fe7a48e6c0','2025-04-19 20:32:06'),(19037466,'1','Paloma','Tamayo','Segura','p.tamayo.segura@gmail.com','+56966744011','Pilmaiquen 1481','00a684800a2e6ab0b17fbc343478846f3ca4fdaf81ea81fc6422e582d35e4064','2025-05-23',1,'vecino',NULL,NULL),(25592802,'3','Batitú','Mayorga','Mendoza','prueba@gmail.com','+56998555466','prueba','30b62cbe41ff0cd5a6cd8ed2ff4f47d4a152b56e0e79587a3758137f58d2bec8','2025-04-19',1,'vecino',NULL,NULL);
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios_backup`
--

DROP TABLE IF EXISTS `usuarios_backup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios_backup` (
  `rut` int NOT NULL,
  `dv_rut` char(1) NOT NULL,
  `nombre` varchar(45) NOT NULL,
  `apellido_paterno` varchar(45) NOT NULL,
  `apellido_materno` varchar(45) DEFAULT NULL,
  `correo_electronico` varchar(45) DEFAULT NULL,
  `telefono` varchar(20) NOT NULL,
  `direccion` varchar(200) NOT NULL,
  `password` varchar(255) NOT NULL,
  `fecha_registro` date NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1' COMMENT '''1: Activo, 0: Inactivo'',',
  `tipo_usuario` varchar(10) NOT NULL DEFAULT 'vecino' COMMENT '''vecino, socio, directiva''',
  `token_recuperacion` varchar(255) DEFAULT NULL,
  `fecha_token_recuperacion` datetime DEFAULT NULL,
  PRIMARY KEY (`rut`),
  UNIQUE KEY `rut_UNIQUE` (`rut`),
  UNIQUE KEY `correo_electronico_UNIQUE` (`correo_electronico`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios_backup`
--

LOCK TABLES `usuarios_backup` WRITE;
/*!40000 ALTER TABLE `usuarios_backup` DISABLE KEYS */;
/*!40000 ALTER TABLE `usuarios_backup` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'vecindapp_bd'
--
/*!50003 DROP PROCEDURE IF EXISTS `SP_ACTUALIZAR_DATOS_USUARIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ACTUALIZAR_DATOS_USUARIO`(
    IN p_rut INT,
    IN p_nombres VARCHAR(100),
    IN p_apellido_paterno VARCHAR(100),
    IN p_apellido_materno VARCHAR(100),
    IN p_telefono VARCHAR(20),
    IN p_correo_electronico VARCHAR(45),
    IN p_direccion VARCHAR(200)
)
BEGIN
    -- Validar que el correo no esté en uso por otro usuario
    IF EXISTS (
        SELECT 1 
        FROM usuarios 
        WHERE correo_electronico = p_correo_electronico AND rut != p_rut
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El correo electrónico ya está en uso por otro usuario';
    END IF;
    
    -- Actualizar datos
    UPDATE usuarios 
    SET 
        nombre = p_nombres,
        apellido_paterno = p_apellido_paterno,
        apellido_materno = p_apellido_materno,
        telefono = p_telefono,
        correo_electronico = p_correo_electronico,
        direccion = p_direccion
    WHERE rut = p_rut;
    
    SELECT 'OK' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_ACTUALIZAR_ESTADO_SOCIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ACTUALIZAR_ESTADO_SOCIO`(
    IN p_idsocio INT,
    IN p_estado TINYINT,
    IN p_motivo VARCHAR(200)
)
BEGIN
    DECLARE v_existe INT;
    DECLARE v_mensaje VARCHAR(100);

    -- Verificar si el socio existe
    SELECT COUNT(*) INTO v_existe
    FROM socios 
    WHERE idsocio = p_idsocio;

    IF v_existe = 0 THEN
        SET v_mensaje = CONCAT('No existe un socio con el ID: ', p_idsocio);
        SELECT v_mensaje as mensaje;
    ELSE
        -- Actualizar el estado del socio
        UPDATE socios
        SET 
            estado = p_estado,
            motivo_desactivacion = CASE 
                WHEN p_estado = 0 THEN p_motivo
                ELSE NULL
            END,
            fecha_desactivacion = CASE 
                WHEN p_estado = 0 THEN NOW()
                ELSE NULL
            END
        WHERE idsocio = p_idsocio;

        -- Preparar mensaje de respuesta
        IF p_estado = 1 THEN
            SET v_mensaje = CONCAT('Socio ', p_idsocio, ' activado correctamente');
        ELSE
            SET v_mensaje = CONCAT('Socio ', p_idsocio, ' desactivado correctamente');
        END IF;

        SELECT v_mensaje as mensaje;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_ACTUALIZAR_EVENTO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ACTUALIZAR_EVENTO`(
    IN p_evento_id INT,
    IN p_titulo VARCHAR(200),
    IN p_descripcion TEXT,
    IN p_fecha_evento DATE,
    IN p_hora_evento TIME,
    IN p_lugar VARCHAR(255),
    IN p_notas TEXT,
    IN p_estado VARCHAR(20)
)
BEGIN
    UPDATE eventos
    SET 
        titulo = p_titulo,
        descripcion = p_descripcion,
        fecha_evento = p_fecha_evento,
        hora_evento = p_hora_evento,
        lugar = p_lugar,
        notas = p_notas,
        estado = p_estado
    WHERE id = p_evento_id;
    
    SELECT ROW_COUNT() AS filas_actualizadas;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_APROBAR_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_APROBAR_CERTIFICADO`(
    IN p_solicitud_id INT,
    IN p_directiva_rut INT,
    IN p_observaciones TEXT
)
BEGIN
    DECLARE v_codigo_verificacion VARCHAR(50);
    DECLARE v_archivo_pdf VARCHAR(255);
    
    -- Generar código de verificación único
    SET v_codigo_verificacion = CONCAT('CERT-', FLOOR(RAND() * 1000000));
    SET v_archivo_pdf = CONCAT('/certificados/', v_codigo_verificacion, '.pdf');

    -- Actualizar solicitud de certificado
    UPDATE solicitudes_certificado
    SET 
        estado = 'aprobado', 
        fecha_aprobacion = NOW(),
        directiva_rut = p_directiva_rut,
        observaciones = p_observaciones
    WHERE id = p_solicitud_id;

    -- Insertar certificado
    INSERT INTO certificados 
    (solicitud_id, codigo_verificacion, fecha_emision, fecha_vencimiento, archivo_pdf)
    VALUES 
    (
        p_solicitud_id, 
        v_codigo_verificacion, 
        NOW(), 
        DATE_ADD(NOW(), INTERVAL 3 MONTH), 
        v_archivo_pdf
    );

    SELECT 
        v_codigo_verificacion AS codigo_verificacion,
        'Certificado aprobado y generado exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_APROBAR_SOLICITUD_SOCIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_APROBAR_SOLICITUD_SOCIO`(
    IN p_rut INT
)
BEGIN
    -- Verificar que la solicitud exista y esté pendiente
    IF NOT EXISTS (
        SELECT 1 FROM socios 
        WHERE rut = p_rut AND estado_solicitud = 'pendiente'
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No existe una solicitud pendiente para este usuario';
    END IF;

    -- Actualizar estado de solicitud
    UPDATE socios
    SET 
        estado_solicitud = 'aprobada',
        fecha_aprobacion = CURRENT_DATE,
        estado = 1
    WHERE rut = p_rut;

    -- Actualizar tipo de usuario
    UPDATE usuarios
    SET tipo_usuario = 'socio'
    WHERE rut = p_rut;

    SELECT 'Solicitud de socio aprobada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_AUTENTICAR_USUARIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_AUTENTICAR_USUARIO`(
    IN p_rut INT,
    IN p_password VARCHAR(255)
)
BEGIN
    DECLARE v_tipo_usuario VARCHAR(10);
    
    -- Validar credenciales
    SELECT tipo_usuario INTO v_tipo_usuario
    FROM usuarios
    WHERE rut = p_rut AND password = p_password AND estado = 1;
    
    IF v_tipo_usuario IS NOT NULL THEN
        SELECT 
            rut, 
            nombre, 
            apellido_paterno, 
            tipo_usuario, 
            'Autenticación exitosa' AS mensaje
        FROM usuarios
        WHERE rut = p_rut;
    ELSE
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Credenciales inválidas o usuario inactivo';
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CAMBIAR_CONTRASENA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CAMBIAR_CONTRASENA`(
    IN p_rut INT,
    IN p_contrasena_actual VARCHAR(255),
    IN p_contrasena_nueva VARCHAR(255)
)
BEGIN
    DECLARE v_contrasena_bd VARCHAR(255);

    -- Verificar contraseña actual
    SELECT password INTO v_contrasena_bd
    FROM usuarios
    WHERE rut = p_rut;

    -- Validar contraseña actual
    IF v_contrasena_bd != p_contrasena_actual THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'La contraseña actual no es correcta';
    END IF;

    -- Actualizar contraseña
    UPDATE usuarios
    SET password = p_contrasena_nueva
    WHERE rut = p_rut;

    SELECT 'Contraseña actualizada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CANCELAR_EVENTO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CANCELAR_EVENTO`(
    IN p_evento_id INT
)
BEGIN
    UPDATE eventos
    SET estado = 'cancelado'
    WHERE id = p_evento_id;
    
    SELECT ROW_COUNT() AS filas_actualizadas;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONFIGURAR_TARIFA_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONFIGURAR_TARIFA_CERTIFICADO`(
    IN p_tipo_certificado_id INT,
    IN p_precio_socio DECIMAL(10,2),
    IN p_precio_vecino DECIMAL(10,2),
    IN p_medios_pago SET('efectivo', 'transferencia', 'webpay', 'otro') 
)
BEGIN
    UPDATE tipos_certificado
    SET 
        precio_socio = p_precio_socio,
        precio_vecino = p_precio_vecino,
        medios_pago_habilitados = p_medios_pago
    WHERE id = p_tipo_certificado_id;

    SELECT 'Tarifas de certificado actualizadas exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONFIRMAR_PAGOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONFIRMAR_PAGOS`(
    IN p_token_webpay VARCHAR(255),
    IN p_estado_pago VARCHAR(20),
    IN p_respuesta_webpay TEXT
)
BEGIN
    DECLARE v_pago_id INT;
    DECLARE v_tipo_transaccion VARCHAR(20);
    DECLARE v_referencia_id INT;

    -- Buscar transacción de WebPay
    SELECT 
        pago_id, 
        tipo_transaccion 
    INTO 
        v_pago_id, 
        v_tipo_transaccion
    FROM transacciones_webpay
    WHERE token_webpay = p_token_webpay;

    -- Actualizar pago
    UPDATE pagos
    SET 
        estado = p_estado_pago,
        respuesta_webpay = p_respuesta_webpay
    WHERE id = v_pago_id;

    -- Actualizar transacción WebPay
    UPDATE transacciones_webpay
    SET 
        estado = p_estado_pago
    WHERE token_webpay = p_token_webpay;

    -- Si el pago fue aprobado, actualizar estado de cuota o certificado
    IF p_estado_pago = 'aprobado' THEN
        IF v_tipo_transaccion = 'cuota' THEN
            UPDATE cuotas_socio
            SET 
                estado = 'pagado',
                pago_id = v_pago_id
            WHERE id = (
                SELECT referencia_id 
                FROM pagos 
                WHERE id = v_pago_id
            );
        ELSEIF v_tipo_transaccion = 'certificado' THEN
            UPDATE solicitudes_certificado
            SET 
                estado = 'pagado'
            WHERE id = (
                SELECT referencia_id 
                FROM pagos 
                WHERE id = v_pago_id
            );
        END IF;
    END IF;

    SELECT 'Pago procesado exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_ASISTENTES` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_ASISTENTES`(
    IN p_evento_id INT
)
BEGIN
    SELECT 
        u.rut,
        u.nombre,
        u.apellido,
        ae.fecha_asistencia
    FROM asistencia_eventos ae
    INNER JOIN usuarios u ON ae.usuario_rut = u.rut
    WHERE ae.evento_id = p_evento_id
    ORDER BY ae.fecha_asistencia;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_ESTADO_PAGOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_ESTADO_PAGOS`(
    IN p_usuario_rut INT,
    IN p_tipo VARCHAR(20),
    IN p_estado VARCHAR(20)
)
BEGIN
    SELECT 
        p.id AS pago_id,
        p.tipo,
        p.referencia_id,
        p.monto,
        p.fecha_pago,
        p.metodo_pago,
        p.estado,
        CASE 
            WHEN p.tipo = 'cuota' THEN cs.fecha_vencimiento
            WHEN p.tipo = 'certificado' THEN sc.fecha_solicitud
        END AS fecha_vencimiento
    FROM pagos p
    LEFT JOIN cuotas_socio cs ON p.tipo = 'cuota' AND p.referencia_id = cs.id
    LEFT JOIN solicitudes_certificado sc ON p.tipo = 'certificado' AND p.referencia_id = sc.id
    WHERE p.usuario_rut = p_usuario_rut
    AND (p_tipo IS NULL OR p.tipo = p_tipo)
    AND (p_estado IS NULL OR p.estado = p_estado)
    ORDER BY p.fecha_pago DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_EVENTO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_EVENTO`(
    IN p_evento_id INT
)
BEGIN
    SELECT 
        e.*,
        u.nombre as nombre_directiva,
        u.apellido_paterno as apellido_directiva,
        COUNT(ae.usuario_rut) as total_asistentes
    FROM eventos e
    LEFT JOIN usuarios u ON e.directiva_rut = u.rut
    LEFT JOIN asistencia_eventos ae ON e.id = ae.evento_id
    WHERE e.id = p_evento_id
    GROUP BY e.id;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_EVENTOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_EVENTOS`(
    IN p_usuario_rut INT
)
BEGIN
    DECLARE v_es_directiva BOOLEAN;
    
    -- Verificar si el usuario es directiva
    SELECT EXISTS(
        SELECT 1 FROM usuarios 
        WHERE rut = p_usuario_rut 
        AND tipo_usuario = 'directiva'
    ) INTO v_es_directiva;
    
    IF v_es_directiva THEN
        -- Directiva ve todos los eventos
        SELECT 
            id,
            titulo,
            descripcion,
            fecha_evento,
            hora_evento,
            lugar,
            directiva_rut,
            estado,
            codigo_qr,
            fecha_creacion,
            notas
        FROM eventos
        ORDER BY fecha_evento DESC, hora_evento DESC;
    ELSE
        -- Vecinos/socios ven solo eventos activos
        SELECT 
            id,
            titulo,
            descripcion,
            fecha_evento,
            hora_evento,
            lugar,
            directiva_rut,
            estado,
            codigo_qr,
            fecha_creacion,
            notas
        FROM eventos
        WHERE estado = 'activo'
        ORDER BY fecha_evento DESC, hora_evento DESC;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_HISTORIAL_ASISTENCIA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_HISTORIAL_ASISTENCIA`(
    IN p_usuario_rut INT
)
BEGIN
    SELECT 
        e.id,
        e.titulo,
        e.fecha_evento,
        e.hora_evento,
        e.lugar,
        ae.fecha_asistencia
    FROM asistencia_eventos ae
    INNER JOIN eventos e ON ae.evento_id = e.id
    WHERE ae.usuario_rut = p_usuario_rut
    ORDER BY e.fecha_evento DESC, e.hora_evento DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_HISTORIAL_CERTIFICADOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_HISTORIAL_CERTIFICADOS`(
    IN p_usuario_rut INT,
    IN p_estado VARCHAR(20)
)
BEGIN
    -- Si no se proporciona estado, traer todos los certificados
    IF p_estado IS NULL OR p_estado = '' THEN
        SELECT 
            sc.id AS id_solicitud,
            u.rut,
            u.nombre,
            u.apellido_paterno,
            tc.nombre AS tipo_certificado,
            sc.fecha_solicitud,
            sc.estado,
            c.codigo_verificacion,
            c.fecha_emision,
            c.fecha_vencimiento
        FROM solicitudes_certificado sc
        JOIN usuarios u ON sc.usuario_rut = u.rut
        JOIN tipos_certificado tc ON sc.tipo_certificado_id = tc.id
        LEFT JOIN certificados c ON sc.id = c.solicitud_id
        WHERE sc.usuario_rut = p_usuario_rut
        ORDER BY sc.fecha_solicitud DESC;
    ELSE
        -- Filtrar por estado de solicitud
        SELECT 
            sc.id AS id_solicitud,
            u.rut,
            u.nombre,
            u.apellido_paterno,
            tc.nombre AS tipo_certificado,
            sc.fecha_solicitud,
            sc.estado,
            c.codigo_verificacion,
            c.fecha_emision,
            c.fecha_vencimiento
        FROM solicitudes_certificado sc
        JOIN usuarios u ON sc.usuario_rut = u.rut
        JOIN tipos_certificado tc ON sc.tipo_certificado_id = tc.id
        LEFT JOIN certificados c ON sc.id = c.solicitud_id
        WHERE sc.usuario_rut = p_usuario_rut AND sc.estado = p_estado
        ORDER BY sc.fecha_solicitud DESC;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_NOTICIAS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_NOTICIAS`(
    IN p_tipo_usuario VARCHAR(10),
    IN p_destacado TINYINT
)
BEGIN
    IF p_tipo_usuario = 'vecino' THEN
        -- Noticias para vecinos (visibilidad todos)
        SELECT 
            id, 
            titulo, 
            contenido, 
            fecha_publicacion, 
            fecha_evento, 
            lugar, 
            imagen,
            destacado
        FROM noticias
        WHERE visibilidad = 'todos' 
        AND (p_destacado IS NULL OR destacado = p_destacado)
        ORDER BY fecha_publicacion DESC;
    
    ELSEIF p_tipo_usuario = 'socio' THEN
        -- Noticias para socios (todos y solo socios)
        SELECT 
            id, 
            titulo, 
            contenido, 
            fecha_publicacion, 
            fecha_evento, 
            lugar, 
            imagen,
            destacado
        FROM noticias
        WHERE visibilidad IN ('todos', 'solo_socios')
        AND (p_destacado IS NULL OR destacado = p_destacado)
        ORDER BY fecha_publicacion DESC;
    
    ELSE
        -- Directiva puede ver todas las noticias
        SELECT 
            id, 
            titulo, 
            contenido, 
            fecha_publicacion, 
            fecha_evento, 
            lugar, 
            imagen,
            visibilidad,
            destacado
        FROM noticias
        ORDER BY fecha_publicacion DESC;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_NOTIFICACIONES` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_NOTIFICACIONES`(
    IN p_usuario_rut INT,
    IN p_leida TINYINT
)
BEGIN
    SELECT 
        id,
        titulo,
        mensaje,
        tipo,
        fecha_creacion,
        leida,
        fecha_lectura,
        referencia_tipo,
        referencia_id
    FROM notificaciones
    WHERE usuario_rut = p_usuario_rut
    AND (p_leida IS NULL OR leida = p_leida)
    ORDER BY fecha_creacion DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CONSULTAR_SOLICITUDES_SOCIOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CONSULTAR_SOLICITUDES_SOCIOS`(
    IN p_estado_solicitud VARCHAR(20)
)
BEGIN
    -- Si no se proporciona estado, traer todas las solicitudes
    IF p_estado_solicitud IS NULL OR p_estado_solicitud = '' THEN
        SELECT 
            s.rut,
            u.nombre,
            u.apellido_paterno,
            u.apellido_materno,
            s.fecha_solicitud,
            s.estado_solicitud,
            s.motivo_rechazo
        FROM socios s
        JOIN usuarios u ON s.rut = u.rut
        ORDER BY s.fecha_solicitud;
    ELSE
        -- Filtrar por estado de solicitud
        SELECT 
            s.rut,
            u.nombre,
            u.apellido_paterno,
            u.apellido_materno,
            s.fecha_solicitud,
            s.estado_solicitud,
            s.motivo_rechazo
        FROM socios s
        JOIN usuarios u ON s.rut = u.rut
        WHERE s.estado_solicitud = p_estado_solicitud
        ORDER BY s.fecha_solicitud;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_CREAR_EVENTO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_CREAR_EVENTO`(
    IN p_titulo VARCHAR(200),
    IN p_descripcion TEXT,
    IN p_fecha_evento DATE,
    IN p_hora_evento TIME,
    IN p_lugar VARCHAR(255),
    IN p_directiva_rut INT,
    IN p_notas TEXT
)
BEGIN
    DECLARE v_codigo_qr VARCHAR(255);
    
    -- Generar código único para el QR
    SET v_codigo_qr = CONCAT('EVENT-', FLOOR(RAND() * 1000000));
    
    INSERT INTO eventos (
        titulo, 
        descripcion, 
        fecha_evento, 
        hora_evento, 
        lugar, 
        directiva_rut, 
        codigo_qr,
        notas,
        estado
    ) VALUES (
        p_titulo,
        p_descripcion,
        p_fecha_evento,
        p_hora_evento,
        p_lugar,
        p_directiva_rut,
        v_codigo_qr,
        p_notas,
        'activo'
    );
    
    SELECT LAST_INSERT_ID() AS id_evento, v_codigo_qr AS codigo_qr;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_ELIMINAR_USUARIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_ELIMINAR_USUARIO`(
    IN p_rut INT,
    IN p_rut_solicitante INT,
    OUT p_mensaje VARCHAR(255)
)
proc_label: BEGIN
    DECLARE usuario_existe INT DEFAULT 0;
    DECLARE usuario_tipo VARCHAR(10);
    DECLARE solicitante_tipo VARCHAR(10);
    DECLARE directiva_count INT;
    DECLARE tiene_solicitud_pendiente INT DEFAULT 0;
    DECLARE tiene_certificados_recientes INT DEFAULT 0;
    
    -- Verificar si el usuario existe
    SELECT COUNT(*) INTO usuario_existe
    FROM usuarios
    WHERE rut = p_rut;
    
    -- Si existe, obtener su tipo de usuario
    IF usuario_existe > 0 THEN
        SELECT tipo_usuario INTO usuario_tipo
        FROM usuarios
        WHERE rut = p_rut;
    ELSE
        SET p_mensaje = 'Usuario no encontrado';
        LEAVE proc_label;
    END IF;
    
    -- Obtener tipo de usuario del solicitante
    SELECT COUNT(*) INTO usuario_existe
    FROM usuarios
    WHERE rut = p_rut_solicitante;
    
    IF usuario_existe > 0 THEN
        SELECT tipo_usuario INTO solicitante_tipo
        FROM usuarios
        WHERE rut = p_rut_solicitante;
    ELSE
        SET p_mensaje = 'Usuario solicitante no encontrado';
        LEAVE proc_label;
    END IF;
    
    -- Verificar permisos de eliminación
    IF p_rut <> p_rut_solicitante AND solicitante_tipo NOT IN ('administrador', 'directiva') THEN
        SET p_mensaje = 'No autorizado para eliminar esta cuenta';
        LEAVE proc_label;
    END IF;
    
    -- Verificar si es el último miembro de la directiva
    IF usuario_tipo = 'directiva' AND p_rut <> p_rut_solicitante THEN
        SELECT COUNT(*) INTO directiva_count FROM usuarios WHERE tipo_usuario = 'directiva';
        
        IF directiva_count <= 1 THEN
            SET p_mensaje = 'No se puede eliminar el único miembro de la directiva';
            LEAVE proc_label;
        END IF;
    END IF;
    
    -- Verificar dependencias
    -- 1. Verificar si el usuario tiene solicitudes de certificado pendientes
    SELECT COUNT(*) INTO tiene_solicitud_pendiente 
    FROM solicitudes_certificado 
    WHERE usuario_rut = p_rut AND estado = 'pendiente';
    
    IF tiene_solicitud_pendiente > 0 THEN
        SET p_mensaje = 'No se puede eliminar al usuario porque tiene solicitudes de certificado pendientes';
        LEAVE proc_label;
    END IF;
    
    -- 2. Verificar si el usuario tiene certificados emitidos recientemente
    SELECT COUNT(*) INTO tiene_certificados_recientes
    FROM certificados c
    JOIN solicitudes_certificado sc ON c.solicitud_id = sc.id
    WHERE sc.usuario_rut = p_rut 
    AND c.fecha_emision > DATE_SUB(NOW(), INTERVAL 3 MONTH)
    AND c.estado = 'vigente';
    
    IF tiene_certificados_recientes > 0 THEN
        SET p_mensaje = 'No se puede eliminar al usuario porque tiene certificados vigentes';
        LEAVE proc_label;
    END IF;
    
    -- Si llegamos aquí, procedemos con la eliminación
    UPDATE usuarios 
    SET estado = 0,
        password = 'DELETED',  -- Usar un valor fijo en lugar de NULL
        token_recuperacion = NULL,
        fecha_token_recuperacion = NULL
    WHERE rut = p_rut;
    
    SET p_mensaje = 'OK';
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_GENERAR_CUOTAS_MENSUALES` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_GENERAR_CUOTAS_MENSUALES`()
BEGIN
    -- Insertar cuotas para socios activos
    INSERT INTO cuotas_socio 
    (idsocio, tipo_cuota_id, fecha_generacion, fecha_vencimiento, monto, estado)
    SELECT 
        s.idsocio, 
        tc.id, 
        CURRENT_DATE, 
        DATE_ADD(CURRENT_DATE, INTERVAL 1 MONTH), 
        tc.monto, 
        'pendiente'
    FROM socios s
    JOIN tipos_cuota tc ON tc.periodicidad = 'mensual'
    WHERE s.estado = 1;

    SELECT 'Cuotas mensuales generadas exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_GENERAR_REPORTE_ASISTENCIA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_GENERAR_REPORTE_ASISTENCIA`(
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE
)
BEGIN
    SELECT 
        e.id,
        e.titulo,
        e.fecha_evento,
        e.hora_evento,
        e.lugar,
        COUNT(ae.usuario_rut) as total_asistentes
    FROM eventos e
    LEFT JOIN asistencia_eventos ae ON e.id = ae.evento_id
    WHERE e.fecha_evento BETWEEN p_fecha_inicio AND p_fecha_fin
    GROUP BY e.id
    ORDER BY e.fecha_evento DESC, e.hora_evento DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_GENERAR_REPORTE_FINANCIERO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_GENERAR_REPORTE_FINANCIERO`(
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE
)
BEGIN
    -- Ingresos por cuotas
    SELECT 
        'Cuotas' AS concepto,
        COUNT(*) AS cantidad,
        SUM(p.monto) AS total_ingresos,
        SUM(CASE WHEN p.estado = 'aprobado' THEN p.monto ELSE 0 END) AS total_ingresos_confirmados
    FROM pagos p
    WHERE p.tipo = 'cuota'
    AND p.fecha_pago BETWEEN p_fecha_inicio AND p_fecha_fin
    
    UNION
    
    -- Ingresos por certificados
    SELECT 
        'Certificados' AS concepto,
        COUNT(*) AS cantidad,
        SUM(p.monto) AS total_ingresos,
        SUM(CASE WHEN p.estado = 'aprobado' THEN p.monto ELSE 0 END) AS total_ingresos_confirmados
    FROM pagos p
    WHERE p.tipo = 'certificado'
    AND p.fecha_pago BETWEEN p_fecha_inicio AND p_fecha_fin;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_GENERAR_TOKEN_RECUPERACION` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_GENERAR_TOKEN_RECUPERACION`(
    IN p_rut INT,
    IN p_correo_electronico VARCHAR(45)
)
BEGIN
    DECLARE v_token VARCHAR(255);
    
    -- Validar que el correo coincida con el RUT
    IF EXISTS (SELECT 1 FROM usuarios WHERE rut = p_rut AND correo_electronico = p_correo_electronico) THEN
        -- Generar token único
        SET v_token = MD5(CONCAT(p_rut, NOW(), RAND()));
        
        -- Actualizar token de recuperación
        UPDATE usuarios
        SET 
            token_recuperacion = v_token,
            fecha_token_recuperacion = NOW()
        WHERE rut = p_rut;
        
        SELECT v_token AS token, 'Token generado exitosamente' AS mensaje;
    ELSE
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Correo electrónico no coincide con el RUT';
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_INICIAR_SESION` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_INICIAR_SESION`(
    IN p_rut INT,
    IN p_password VARCHAR(255),
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DECLARE v_usuario_existe INT;
    DECLARE v_password_correcta INT;

    -- Verificar si el usuario existe
    SELECT COUNT(*) INTO v_usuario_existe
    FROM usuarios
    WHERE rut = p_rut;

    -- Si el usuario no existe
    IF v_usuario_existe = 0 THEN
        SET p_mensaje = 'Usuario no encontrado';
    ELSE
        -- Verificar si la contraseña es correcta
        SELECT COUNT(*) INTO v_password_correcta
        FROM usuarios
        WHERE rut = p_rut AND password = p_password;

        -- Verificar estado del usuario
        IF v_password_correcta = 0 THEN
            SET p_mensaje = 'Contraseña incorrecta';
        ELSE
            -- Verificar si el usuario está activo
            SELECT COUNT(*) INTO v_usuario_existe
            FROM usuarios
            WHERE rut = p_rut AND estado = 1;

            IF v_usuario_existe = 0 THEN
                SET p_mensaje = 'Usuario inactivo';
            ELSE
                SET p_mensaje = 'OK';
            END IF;
        END IF;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_MARCAR_NOTIFICACIONES_LEIDAS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_MARCAR_NOTIFICACIONES_LEIDAS`(
    IN p_usuario_rut INT,
    IN p_id_notificacion INT
)
BEGIN
    -- Si se proporciona un ID específico
    IF p_id_notificacion IS NOT NULL THEN
        UPDATE notificaciones
        SET 
            leida = 1,
            fecha_lectura = NOW()
        WHERE id = p_id_notificacion AND usuario_rut = p_usuario_rut;
    ELSE
        -- Marcar todas las notificaciones del usuario como leídas
        UPDATE notificaciones
        SET 
            leida = 1,
            fecha_lectura = NOW()
        WHERE usuario_rut = p_usuario_rut AND leida = 0;
    END IF;

    SELECT 'Notificaciones marcadas como leídas' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_obtener_estadisticas_socios` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_obtener_estadisticas_socios`()
BEGIN
    -- Declaramos variables para las estadísticas
    DECLARE total_socios INT;
    DECLARE solicitudes_pendientes INT;
    DECLARE socios_activos INT;
    DECLARE socios_inactivos INT;
    
    -- Obtenemos el total de socios
    SELECT COUNT(*) INTO total_socios 
    FROM socios;
    
    -- Obtenemos solicitudes pendientes (usando la tabla socios)
    SELECT COUNT(*) INTO solicitudes_pendientes 
    FROM socios 
    WHERE estado_solicitud = 'pendiente';
    
    -- Obtenemos socios activos
    SELECT COUNT(*) INTO socios_activos 
    FROM socios 
    WHERE estado = 1;
    
    -- Obtenemos socios inactivos
    SELECT COUNT(*) INTO socios_inactivos 
    FROM socios 
    WHERE estado = 0 AND estado_solicitud = 'aprobada';
    
    -- Retornamos las estadísticas
    SELECT 
        total_socios,
        solicitudes_pendientes,
        socios_activos,
        socios_inactivos;
        
    -- Retornamos las últimas 5 actividades relacionadas con socios
    SELECT 
        ra.id,
        ra.accion as titulo,
        ra.detalles as descripcion,
        ra.fecha_hora as fecha,
        CASE 
            WHEN ra.accion LIKE '%crear%' THEN 'person-add-outline'
            WHEN ra.accion LIKE '%actualizar%' THEN 'create-outline'
            WHEN ra.accion LIKE '%eliminar%' THEN 'trash-outline'
            WHEN ra.accion LIKE '%pago%' THEN 'cash-outline'
            ELSE 'document-text-outline'
        END as icono,
        CASE 
            WHEN ra.accion LIKE '%crear%' THEN 'success'
            WHEN ra.accion LIKE '%actualizar%' THEN 'primary'
            WHEN ra.accion LIKE '%eliminar%' THEN 'danger'
            WHEN ra.accion LIKE '%pago%' THEN 'success'
            ELSE 'warning'
        END as color
    FROM registro_actividades ra
    WHERE ra.entidad IN ('socio', 'solicitud_membresia')
    ORDER BY ra.fecha_hora DESC
    LIMIT 5;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_OBTENER_HISTORIAL_SOCIOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_OBTENER_HISTORIAL_SOCIOS`()
BEGIN
    SELECT 
        u.rut,
        u.dv_rut,
        u.nombre,
        u.apellido_paterno,
        u.apellido_materno,
        u.correo_electronico,
        u.telefono,
        u.direccion,
        u.fecha_registro,
        s.idsocio,
        s.fecha_solicitud,
        s.fecha_aprobacion,
        s.estado,
        s.motivo_desactivacion,
        s.fecha_desactivacion
    FROM 
        usuarios u
    JOIN 
        socios s ON u.rut = s.rut
    WHERE 
        s.estado_solicitud = 'aprobada'
        AND u.tipo_usuario IN ('socio', 'directiva')
    ORDER BY 
        s.fecha_aprobacion DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_OBTENER_SOCIOS_ACTIVOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_OBTENER_SOCIOS_ACTIVOS`()
BEGIN
    SELECT 
        u.rut,
        u.dv_rut,
        u.nombre,
        u.apellido_paterno,
        u.apellido_materno,
        u.correo_electronico,
        u.telefono,
        u.direccion,
        u.fecha_registro,
        s.idsocio,
        s.fecha_solicitud,
        s.fecha_aprobacion,
        s.estado,
        s.motivo_desactivacion
    FROM 
        usuarios u
    JOIN 
        socios s ON u.rut = s.rut
    WHERE 
        s.estado = 1 
        AND s.estado_solicitud = 'aprobada'
        AND u.tipo_usuario IN ('socio', 'directiva')
    ORDER BY 
        s.fecha_aprobacion DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_OBTENER_SOLICITUDES_PENDIENTES` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_OBTENER_SOLICITUDES_PENDIENTES`()
BEGIN
    SELECT 
        sc.id,
        sc.usuario_rut,
        sc.tipo_certificado_id,
        sc.fecha_solicitud,
        sc.estado,
        sc.motivo,
        sc.documentos_adjuntos,
        sc.fecha_aprobacion,
        sc.directiva_rut,
        sc.precio,
        sc.observaciones
    FROM solicitudes_certificado sc
    WHERE sc.estado = 'pendiente'
    ORDER BY sc.fecha_solicitud DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_OBTENER_TODOS_SOCIOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_OBTENER_TODOS_SOCIOS`()
BEGIN
    SELECT 
        u.rut,
        u.dv_rut,
        u.nombre,
        u.apellido_paterno,
        u.apellido_materno,
        u.correo_electronico,
        u.telefono,
        u.direccion,
        u.fecha_registro,
        s.idsocio,
        s.fecha_solicitud,
        s.fecha_aprobacion,
        s.estado,
        s.motivo_desactivacion
    FROM 
        usuarios u
    JOIN 
        socios s ON u.rut = s.rut
    WHERE 
        s.estado_solicitud = 'aprobada'
        AND u.tipo_usuario IN ('socio', 'directiva')
    ORDER BY 
        s.fecha_aprobacion DESC;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_PREREGISTRAR_SOCIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_PREREGISTRAR_SOCIO`(
    IN p_rut INT,
    IN p_idsocio INT,
    IN p_fecha_solicitud DATE,
    IN p_fecha_aprobacion DATE
)
BEGIN
    DECLARE v_usuario_existe INT;
    
    -- Verificar si el usuario ya existe
    SELECT COUNT(*) INTO v_usuario_existe FROM usuarios WHERE rut = p_rut;
    
    -- Si el usuario no existe, crear un registro básico
    IF v_usuario_existe = 0 THEN
        INSERT INTO usuarios (
            rut, 
            dv_rut, 
            nombre, 
            apellido_paterno,
            correo_electronico,
            telefono,
            direccion,
            password,
            fecha_registro,
            estado,
            tipo_usuario
        ) VALUES (
            p_rut,
            '-', -- Placeholder para dv_rut
            'Socio Preregistrado', -- Placeholder para nombre
            'Pendiente Activación', -- Placeholder para apellido
            CONCAT(p_rut, '@preregistro.temp'), -- Correo temporal único
            '-', -- Placeholder para teléfono
            '-', -- Placeholder para dirección
            'PREREGISTRADO', -- Password temporal que no es válido para login
            CURRENT_DATE,
            0, -- Estado inactivo hasta que completen el registro
            'socio'
        );
    END IF;
    
    -- Verificar si ya existe el socio con ese ID
    IF NOT EXISTS (SELECT 1 FROM socios WHERE idsocio = p_idsocio) THEN
        -- Insertar en socios
        INSERT INTO socios (
            idsocio,
            rut,
            fecha_solicitud,
            fecha_aprobacion,
            estado_solicitud,
            estado
        ) VALUES (
            p_idsocio,
            p_rut,
            IFNULL(p_fecha_solicitud, CURRENT_DATE),
            IFNULL(p_fecha_aprobacion, CURRENT_DATE),
            'aprobada',
            1
        );
    ELSE
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe un socio con ese ID';
    END IF;
    
    SELECT 'Socio preregistrado exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_PUBLICAR_NOTICIA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_PUBLICAR_NOTICIA`(
    IN p_titulo VARCHAR(200),
    IN p_contenido TEXT,
    IN p_publicado_por INT,
    IN p_fecha_evento DATETIME,
    IN p_lugar VARCHAR(255),
    IN p_imagen VARCHAR(255),
    IN p_visibilidad VARCHAR(20),
    IN p_destacado TINYINT
)
BEGIN
    DECLARE v_id_noticia INT;

    INSERT INTO noticias 
    (titulo, contenido, publicado_por, fecha_evento, lugar, imagen, visibilidad, destacado)
    VALUES 
    (p_titulo, p_contenido, p_publicado_por, p_fecha_evento, p_lugar, p_imagen, p_visibilidad, p_destacado);

    SET v_id_noticia = LAST_INSERT_ID();

    -- Crear notificaciones para usuarios según visibilidad
    IF p_visibilidad = 'todos' THEN
        INSERT INTO notificaciones (usuario_rut, titulo, mensaje, tipo, referencia_tipo, referencia_id)
        SELECT rut, p_titulo, 'Nueva noticia publicada', 'noticia', 'noticias', v_id_noticia
        FROM usuarios;
    ELSEIF p_visibilidad = 'solo_socios' THEN
        INSERT INTO notificaciones (usuario_rut, titulo, mensaje, tipo, referencia_tipo, referencia_id)
        SELECT rut, p_titulo, 'Nueva noticia exclusiva para socios', 'noticia', 'noticias', v_id_noticia
        FROM usuarios
        WHERE tipo_usuario = 'socio';
    END IF;

    SELECT v_id_noticia AS id_noticia, 'Noticia publicada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_RECHAZAR_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_RECHAZAR_CERTIFICADO`(
    IN p_solicitud_id INT,
    IN p_directiva_rut INT,
    IN p_motivo_rechazo TEXT
)
BEGIN
    -- Actualizar solicitud de certificado
    UPDATE solicitudes_certificado
    SET 
        estado = 'rechazado', 
        fecha_aprobacion = NOW(),
        directiva_rut = p_directiva_rut,
        observaciones = p_motivo_rechazo
    WHERE id = p_solicitud_id;

    SELECT 'Solicitud de certificado rechazada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_RECHAZAR_SOLICITUD_SOCIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_RECHAZAR_SOLICITUD_SOCIO`(
    IN p_rut INT,
    IN p_motivo_rechazo VARCHAR(200)
)
BEGIN
    -- Verificar que la solicitud exista y esté pendiente
    IF NOT EXISTS (
        SELECT 1 FROM socios 
        WHERE rut = p_rut AND estado_solicitud = 'pendiente'
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No existe una solicitud pendiente para este usuario';
    END IF;

    -- Actualizar estado de solicitud
    UPDATE socios
    SET 
        estado_solicitud = 'rechazada',
        motivo_rechazo = p_motivo_rechazo,
        estado = 0
    WHERE rut = p_rut;

    SELECT 'Solicitud de socio rechazada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REGISTRAR_ASISTENCIA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REGISTRAR_ASISTENCIA`(
    IN p_codigo_qr VARCHAR(255),
    IN p_usuario_rut INT
)
BEGIN
    DECLARE v_evento_id INT;
    
    -- Verificar que el evento existe y está activo
    SELECT id INTO v_evento_id
    FROM eventos
    WHERE codigo_qr = p_codigo_qr
    AND estado = 'activo';
    
    IF v_evento_id IS NOT NULL THEN
        -- Verificar que no haya asistencia previa
        IF NOT EXISTS (
            SELECT 1 FROM asistencia_eventos 
            WHERE evento_id = v_evento_id 
            AND usuario_rut = p_usuario_rut
        ) THEN
            INSERT INTO asistencia_eventos (evento_id, usuario_rut)
            VALUES (v_evento_id, p_usuario_rut);
            
            SELECT 'Asistencia registrada exitosamente' AS mensaje;
        ELSE
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Ya existe un registro de asistencia para este usuario';
        END IF;
    ELSE
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Código QR inválido o evento inactivo';
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REGISTRAR_PAGO_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REGISTRAR_PAGO_CERTIFICADO`(
    IN p_usuario_rut INT,
    IN p_solicitud_certificado_id INT,
    IN p_monto DECIMAL(10,2),
    IN p_metodo_pago VARCHAR(20),
    IN p_token_webpay VARCHAR(255),
    IN p_url_pago VARCHAR(255)
)
BEGIN
    DECLARE v_tipo_certificado_id INT;
    DECLARE v_monto_certificado DECIMAL(10,2);
    DECLARE v_pago_id INT;

    -- Obtener monto del certificado
    SELECT 
        tipo_certificado_id, 
        precio INTO v_tipo_certificado_id, v_monto_certificado
    FROM solicitudes_certificado
    WHERE id = p_solicitud_certificado_id;

    -- Validar que el monto pagado coincida
    IF p_monto != v_monto_certificado THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El monto del pago no coincide con el valor del certificado';
    END IF;

    -- Registrar pago inicial
    INSERT INTO pagos 
    (
        usuario_rut, 
        tipo, 
        referencia_id, 
        monto, 
        metodo_pago, 
        estado,
        token_webpay,
        url_pago_webpay
    )
    VALUES 
    (
        p_usuario_rut, 
        'certificado', 
        p_solicitud_certificado_id, 
        p_monto, 
        p_metodo_pago, 
        'procesando',
        p_token_webpay,
        p_url_pago
    );

    -- Obtener ID del pago
    SET v_pago_id = LAST_INSERT_ID();

    -- Registrar transacción de WebPay
    INSERT INTO transacciones_webpay
    (
        pago_id,
        token_webpay,
        monto,
        estado,
        tipo_transaccion,
        usuario_rut
    )
    VALUES
    (
        v_pago_id,
        p_token_webpay,
        p_monto,
        'iniciada',
        'certificado',
        p_usuario_rut
    );

    SELECT v_pago_id AS pago_id, p_token_webpay AS token_webpay, p_url_pago AS url_pago;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REGISTRAR_PAGO_CUOTA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REGISTRAR_PAGO_CUOTA`(
    IN p_usuario_rut INT,
    IN p_cuota_id INT,
    IN p_monto DECIMAL(10,2),
    IN p_metodo_pago VARCHAR(20),
    IN p_token_webpay VARCHAR(255),
    IN p_url_pago VARCHAR(255)
)
BEGIN
    DECLARE v_idsocio INT;
    DECLARE v_monto_cuota DECIMAL(10,2);
    DECLARE v_pago_id INT;

    -- Obtener ID de socio
    SELECT idsocio INTO v_idsocio 
    FROM socios 
    WHERE rut = p_usuario_rut AND estado = 1;

    -- Verificar monto de la cuota
    SELECT monto INTO v_monto_cuota
    FROM cuotas_socio
    WHERE id = p_cuota_id AND idsocio = v_idsocio;

    -- Validar que el monto pagado coincida
    IF p_monto != v_monto_cuota THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El monto del pago no coincide con el valor de la cuota';
    END IF;

    -- Registrar pago inicial
    INSERT INTO pagos 
    (
        usuario_rut, 
        tipo, 
        referencia_id, 
        monto, 
        metodo_pago, 
        estado,
        token_webpay,
        url_pago_webpay
    )
    VALUES 
    (
        p_usuario_rut, 
        'cuota', 
        p_cuota_id, 
        p_monto, 
        p_metodo_pago, 
        'procesando',
        p_token_webpay,
        p_url_pago
    );

    -- Obtener ID del pago
    SET v_pago_id = LAST_INSERT_ID();

    -- Registrar transacción de WebPay
    INSERT INTO transacciones_webpay
    (
        pago_id,
        token_webpay,
        monto,
        estado,
        tipo_transaccion,
        usuario_rut
    )
    VALUES
    (
        v_pago_id,
        p_token_webpay,
        p_monto,
        'iniciada',
        'cuota',
        p_usuario_rut
    );

    SELECT v_pago_id AS pago_id, p_token_webpay AS token_webpay, p_url_pago AS url_pago;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REGISTRAR_TRANSACCION_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REGISTRAR_TRANSACCION_CERTIFICADO`(
    IN p_solicitud_id INT,
    IN p_preferencia_id VARCHAR(255),
    IN p_monto DECIMAL(10,2),
    IN p_estado VARCHAR(20)
)
BEGIN
    INSERT INTO transacciones_certificado (
        solicitud_id,
        preferencia_id,
        monto,
        estado,
        fecha_transaccion
    ) VALUES (
        p_solicitud_id,
        p_preferencia_id,
        p_monto,
        p_estado,
        NOW()
    );
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REGISTRAR_USUARIOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REGISTRAR_USUARIOS`(
    IN p_rut INT,
    IN p_dv_rut CHAR(1),
    IN p_nombre VARCHAR(45),
    IN p_apellido_paterno VARCHAR(45),
    IN p_apellido_materno VARCHAR(45),
    IN p_correo_electronico VARCHAR(45),
    IN p_telefono VARCHAR(20),
    IN p_direccion VARCHAR(200),
    IN p_password VARCHAR(255)
)
BEGIN
    -- Declarar variable para manejar errores
    DECLARE error_msg VARCHAR(255);

    -- Validar que el RUT no esté vacío
    IF p_rut IS NULL THEN
        SET error_msg = 'El RUT no puede estar vacío';
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = error_msg;
    END IF;

    -- Validar que el correo electrónico no esté en uso
    IF EXISTS (SELECT 1 FROM usuarios WHERE correo_electronico = p_correo_electronico) THEN
        SET error_msg = 'El correo electrónico ya está registrado';
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = error_msg;
    END IF;

    -- Validar que el RUT no esté en uso
    IF EXISTS (SELECT 1 FROM usuarios WHERE rut = p_rut) THEN
        SET error_msg = 'El RUT ya está registrado en el sistema';
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = error_msg;
    END IF;

    -- Insertar nuevo usuario
    INSERT INTO usuarios 
    (
        rut, 
        dv_rut, 
        nombre, 
        apellido_paterno, 
        apellido_materno, 
        correo_electronico, 
        telefono, 
        direccion, 
        password, 
        fecha_registro, 
        estado, 
        tipo_usuario
    )
    VALUES 
    (
        p_rut, 
        p_dv_rut, 
        p_nombre, 
        p_apellido_paterno, 
        p_apellido_materno,
        p_correo_electronico, 
        p_telefono, 
        p_direccion, 
        p_password,  -- Nota: Idealmente esto debería ser un hash de contraseña
        CURRENT_DATE, 
        1,  -- Estado activo 
        'vecino'  -- Tipo de usuario por defecto
    );

    -- Devolver mensaje de éxito
    SELECT 'Usuario registrado exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REGISTRAR_USUARIO_EXISTENTE` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REGISTRAR_USUARIO_EXISTENTE`(
    IN p_rut INT,
    IN p_dv_rut CHAR(1),
    IN p_nombre VARCHAR(45),
    IN p_apellido_paterno VARCHAR(45),
    IN p_apellido_materno VARCHAR(45),
    IN p_correo_electronico VARCHAR(45),
    IN p_telefono VARCHAR(20),
    IN p_direccion VARCHAR(200),
    IN p_password VARCHAR(255),
    OUT p_mensaje VARCHAR(255)
)
BEGIN
    DECLARE usuario_existe INT DEFAULT 0;
    DECLARE usuario_activo INT DEFAULT 0;
    
    -- Verificar si el usuario existe
    SELECT COUNT(*) INTO usuario_existe FROM usuarios WHERE rut = p_rut;
    
    -- Verificar si el usuario está activo
    SELECT COUNT(*) INTO usuario_activo FROM usuarios WHERE rut = p_rut AND estado = 1;
    
    -- Si el usuario está activo, no permitir un nuevo registro
    IF usuario_activo > 0 THEN
        SET p_mensaje = 'El RUT ya está registrado como usuario activo en el sistema';
    -- Si el usuario existe pero está inactivo, reactivarlo
    ELSEIF usuario_existe > 0 THEN
        -- Comprobar si el correo ya está siendo usado por otro usuario activo
        IF EXISTS (SELECT 1 FROM usuarios WHERE correo_electronico = p_correo_electronico AND rut != p_rut AND estado = 1) THEN
            SET p_mensaje = 'El correo electrónico ya está registrado por otro usuario activo';
        ELSE
            -- Actualizar datos del usuario y reactivarlo
            UPDATE usuarios 
            SET dv_rut = p_dv_rut,
                nombre = p_nombre,
                apellido_paterno = p_apellido_paterno,
                apellido_materno = p_apellido_materno,
                correo_electronico = p_correo_electronico,
                telefono = p_telefono,
                direccion = p_direccion,
                password = p_password,
                fecha_registro = CURRENT_DATE,
                estado = 1,
                tipo_usuario = 'vecino' -- Por defecto, reingresa como vecino
            WHERE rut = p_rut;
            
            -- Si era socio antes, verificar y restaurar si es posible
            IF EXISTS (SELECT 1 FROM socios WHERE rut = p_rut AND estado_solicitud = 'aprobada') THEN
                -- Restaurar estado de socio si estaba aprobado previamente
                UPDATE socios SET estado = 1 WHERE rut = p_rut AND estado_solicitud = 'aprobada';
                
                -- Actualizar tipo de usuario a socio
                UPDATE usuarios SET tipo_usuario = 'socio' WHERE rut = p_rut;
            END IF;
            
            SET p_mensaje = 'Usuario reactivado exitosamente';
        END IF;
    ELSE
        -- Es un usuario completamente nuevo, usar el procedimiento estándar de registro
        CALL SP_REGISTRAR_USUARIOS(
            p_rut,
            p_dv_rut,
            p_nombre,
            p_apellido_paterno,
            p_apellido_materno,
            p_correo_electronico,
            p_telefono,
            p_direccion,
            p_password
        );
        
        SET p_mensaje = 'Usuario registrado exitosamente';
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_RESTABLECER_CONTRASENA` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_RESTABLECER_CONTRASENA`(
    IN p_rut INT,
    IN p_token VARCHAR(255),
    IN p_nueva_password VARCHAR(255)
)
BEGIN
    -- Validar token y que no haya expirado (válido por 1 hora)
    IF EXISTS (
        SELECT 1 FROM usuarios 
        WHERE rut = p_rut 
        AND token_recuperacion = p_token 
        AND fecha_token_recuperacion > DATE_SUB(NOW(), INTERVAL 1 HOUR)
    ) THEN
        -- Actualizar contraseña y limpiar token
        UPDATE usuarios
        SET 
            password = p_nueva_password,
            token_recuperacion = NULL,
            fecha_token_recuperacion = NULL
        WHERE rut = p_rut;
        
        SELECT 'Contraseña restablecida exitosamente' AS mensaje;
    ELSE
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Token inválido o expirado';
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_REVOCAR_MEMBRESIA_SOCIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_REVOCAR_MEMBRESIA_SOCIO`(
    IN p_rut INT,
    IN p_motivo_revocacion VARCHAR(200)
)
BEGIN
    -- Verificar que el usuario sea socio activo
    IF NOT EXISTS (
        SELECT 1 FROM socios 
        WHERE rut = p_rut AND estado_solicitud = 'aprobada' AND estado = 1
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El usuario no es un socio activo';
    END IF;

    -- Actualizar estado de socio
    UPDATE socios
    SET 
        estado = 0,
        estado_solicitud = 'revocada',
        motivo_rechazo = p_motivo_revocacion
    WHERE rut = p_rut;

    -- Actualizar tipo de usuario
    UPDATE usuarios
    SET tipo_usuario = 'vecino'
    WHERE rut = p_rut;

    SELECT 'Membresía de socio revocada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_SELECT_SOCIOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_SELECT_SOCIOS`(
    IN p_idsocio INT
)
BEGIN
    IF p_idsocio IS NULL OR p_idsocio = 0 THEN
        -- Si no se proporciona un ID o es 0, devuelve todos los socios
        SELECT 
            idsocio,
            rut,
            fecha_solicitud,
            fecha_aprobacion,
            estado_solicitud,
            motivo_rechazo,
            documento_identidad,
            documento_domicilio,
            estado
        FROM socios;
    ELSE
        -- Si se proporciona un ID, devuelve solo ese socio
        SELECT 
            idsocio,
            rut,
            fecha_solicitud,
            fecha_aprobacion,
            estado_solicitud,
            motivo_rechazo,
            documento_identidad,
            documento_domicilio,
            estado
        FROM socios
        WHERE idsocio = p_idsocio;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_SELECT_USUARIOS` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_SELECT_USUARIOS`(
    IN p_rut INT
)
BEGIN
    IF p_rut IS NULL OR p_rut = 0 THEN
        -- Si no se proporciona un RUT o es 0, devuelve todos los usuarios
        SELECT 
            rut,
            dv_rut,
            nombre,
            apellido_paterno,
            apellido_materno, 
            correo_electronico,
            telefono,
            direccion,
            password,
            fecha_registro,
            estado,
            tipo_usuario,
            token_recuperacion,
            fecha_token_recuperacion
        FROM usuarios;
        
    ELSE
        -- Si se proporciona un RUT, devuelve solo ese usuario
        SELECT 
            rut,
            dv_rut,
            nombre,
            apellido_paterno,
            apellido_materno, 
            correo_electronico,
            telefono,
            direccion,
            password,
            fecha_registro,
            estado,
            tipo_usuario,
            token_recuperacion,
            fecha_token_recuperacion
        FROM usuarios
        WHERE rut = p_rut;
    END IF;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_SOLICITAR_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_SOLICITAR_CERTIFICADO`(
    IN p_usuario_rut INT,
    IN p_tipo_certificado_id INT,
    IN p_motivo TEXT,
    IN p_documentos_adjuntos VARCHAR(255)
)
BEGIN
    DECLARE v_precio DECIMAL(10,2);
    DECLARE v_id_solicitud INT;

    -- Determinar precio según si es socio o vecino
    SELECT 
        CASE WHEN u.tipo_usuario = 'socio' THEN tc.precio_socio ELSE tc.precio_vecino END 
    INTO v_precio
    FROM tipos_certificado tc
    JOIN usuarios u ON u.rut = p_usuario_rut
    WHERE tc.id = p_tipo_certificado_id;

    -- Insertar solicitud de certificado
    INSERT INTO solicitudes_certificado 
    (usuario_rut, tipo_certificado_id, fecha_solicitud, estado, motivo, documentos_adjuntos, precio)
    VALUES 
    (p_usuario_rut, p_tipo_certificado_id, NOW(), 'pendiente', p_motivo, p_documentos_adjuntos, v_precio);

    -- Obtener el ID de la solicitud recién insertada
    SET v_id_solicitud = LAST_INSERT_ID();

    SELECT v_id_solicitud AS id_solicitud, 
           'Solicitud de certificado registrada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_SOLICITAR_MEMBRESIA_SOCIO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_SOLICITAR_MEMBRESIA_SOCIO`(
    IN p_rut INT,
    IN p_ruta_documento_identidad VARCHAR(255),
    IN p_ruta_documento_domicilio VARCHAR(255)
)
BEGIN
    -- Verificar que el usuario no sea ya socio
    IF EXISTS (SELECT 1 FROM socios WHERE rut = p_rut) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El usuario ya tiene una solicitud de socio o es socio activo';
    END IF;

    -- Insertar solicitud de membresía
    INSERT INTO socios 
    (rut, fecha_solicitud, estado_solicitud, documento_identidad, documento_domicilio)
    VALUES 
    (p_rut, CURRENT_DATE, 'pendiente', p_ruta_documento_identidad, p_ruta_documento_domicilio);

    SELECT 'Solicitud de membresía registrada exitosamente' AS mensaje;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `SP_VERIFICAR_CERTIFICADO` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `SP_VERIFICAR_CERTIFICADO`(
    IN p_codigo_verificacion VARCHAR(50)
)
BEGIN
    SELECT 
        c.id AS id_certificado,
        c.codigo_verificacion,
        c.fecha_emision,
        c.fecha_vencimiento,
        c.estado,
        sc.usuario_rut,
        u.nombre,
        u.apellido_paterno,
        tc.nombre AS tipo_certificado
    FROM certificados c
    JOIN solicitudes_certificado sc ON c.solicitud_id = sc.id
    JOIN usuarios u ON sc.usuario_rut = u.rut
    JOIN tipos_certificado tc ON sc.tipo_certificado_id = tc.id
    WHERE c.codigo_verificacion = p_codigo_verificacion
    AND c.estado = 'vigente'
    AND c.fecha_vencimiento > NOW();
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-05-26 16:45:13
