-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: bakerydb
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `address`
--

DROP TABLE IF EXISTS `address`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `address` (
  `AddressId` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `AddressLine` varchar(255) DEFAULT NULL,
  `District` varchar(100) DEFAULT NULL,
  `Province` varchar(100) DEFAULT NULL,
  `PostalCode` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`AddressId`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `address_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `address`
--

LOCK TABLES `address` WRITE;
/*!40000 ALTER TABLE `address` DISABLE KEYS */;
INSERT INTO `address` VALUES (1,3,'123 ถนนสุขุมวิท','คลองเตย','กรุงเทพมหานคร','10110'),(2,4,'456 ถนนพระราม 9','ห้วยขวาง','กรุงเทพมหานคร','10310'),(5,7,'plum','bkk','bkk','112332'),(7,6,'456 ถนนพระราม 9','ห้วยขวาง','กรุงเทพมหานคร','10310');
/*!40000 ALTER TABLE `address` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `category`
--

DROP TABLE IF EXISTS `category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `category` (
  `CategoryId` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) NOT NULL,
  `Description` text,
  PRIMARY KEY (`CategoryId`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `category`
--

LOCK TABLES `category` WRITE;
/*!40000 ALTER TABLE `category` DISABLE KEYS */;
INSERT INTO `category` VALUES (1,'เค้ก','ขนมเค้กหลากหลายรูปแบบ'),(2,'คุกกี้','คุกกี้กรอบอร่อย'),(3,'ขนมปัง','ขนมปังสดใหม่ทุกวัน'),(4,'เครื่องดื่ม','เครื่องดื่มร้อนและเย็น'),(8,'1','1'),(9,'1','1'),(10,'1','1');
/*!40000 ALTER TABLE `category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `historyorder`
--

DROP TABLE IF EXISTS `historyorder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `historyorder` (
  `HistoryOrderId` int NOT NULL AUTO_INCREMENT,
  `OrderId` int DEFAULT NULL,
  `UserId` int DEFAULT NULL,
  `OrderDate` datetime DEFAULT NULL,
  `CompletedAt` datetime DEFAULT NULL,
  `TotalAmount` decimal(10,2) DEFAULT NULL,
  `Status` varchar(50) DEFAULT NULL,
  `PaymentStatus` varchar(50) DEFAULT NULL,
  `DeliveryAddress` varchar(500) DEFAULT NULL,
  `ItemSummary` text,
  PRIMARY KEY (`HistoryOrderId`),
  KEY `OrderId` (`OrderId`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `historyorder_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `historyorder`
--

LOCK TABLES `historyorder` WRITE;
/*!40000 ALTER TABLE `historyorder` DISABLE KEYS */;
INSERT INTO `historyorder` VALUES (1,20,6,'2026-03-20 03:08:15','2026-03-20 03:09:30',108.00,'Completed','Paid','456 ถนนพระราม 9, ห้วยขวาง, กรุงเทพมหานคร, 10310','คุกกี้เนย x1'),(2,18,7,'2026-03-20 02:30:15','2026-03-20 04:50:42',350.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1'),(3,17,7,'2026-03-20 01:58:14','2026-03-20 04:51:16',315.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1'),(4,16,7,'2026-03-20 01:43:11','2026-03-20 04:51:25',80.00,'Completed','Paid','plum, bkk, bkk, 112332','กาแฟลาเต้ x1'),(5,21,7,'2026-03-20 04:53:42','2026-03-20 04:55:12',984.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1, เค้กวานิลลา x1, คุกกี้เนย x1, ขนมปังกระเทียม x1, ขนมปังโฮลวีท x1, คุกกี้ช็อคโกแลต x1, ชาร้อน x1, กาแฟลาเต้ x1'),(6,22,6,'2026-03-20 23:18:32','2026-03-21 03:22:53',846.00,'Completed','Paid','456 ถนนพระราม 9, ห้วยขวาง, กรุงเทพมหานคร, 10310','เค้กช็อคโกแลต x1, เค้กวานิลลา x1, คุกกี้เนย x1, คุกกี้ช็อคโกแลต x1'),(7,23,6,'2026-03-21 03:23:53','2026-03-21 03:25:00',1090.00,'Completed','Paid','456 ถนนพระราม 9, ห้วยขวาง, กรุงเทพมหานคร, 10310','เค้กวานิลลา x1, เค้กช็อคโกแลต x1, คุกกี้เนย x1, คุกกี้ช็อคโกแลต x1, ขนมปังโฮลวีท x1, ขนมปังกระเทียม x1'),(8,24,7,'2026-03-28 06:31:22','2026-03-28 06:32:50',145.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กวานิลลา x1'),(9,26,7,'2026-03-28 07:00:20','2026-03-28 07:01:33',940.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1, คุกกี้เนย x1, เค้กวานิลลา x1, คุกกี้ช็อคโกแลต x1, เค้กวานิลลา x1'),(10,25,7,'2026-03-28 06:34:05','2026-03-28 07:02:29',470.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1, คุกกี้เนย x1, เค้กวานิลลา x1'),(11,27,7,'2026-03-28 18:33:20','2026-03-28 18:34:50',620.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1, คุกกี้เนย x1, คุกกี้ช็อคโกแลต x1, เค้กวานิลลา x1'),(12,28,7,'2026-03-28 19:06:05','2026-03-28 19:07:58',711.00,'Completed','Paid','plum, bkk, bkk, 112332','เค้กช็อคโกแลต x1, คุกกี้เนย x1, เค้กวานิลลา x1');
/*!40000 ALTER TABLE `historyorder` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order`
--

DROP TABLE IF EXISTS `order`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order` (
  `OrderId` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `AddressId` int DEFAULT NULL,
  `OrderDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `TotalAmount` decimal(10,2) DEFAULT NULL,
  `Status` enum('Pending','Paid','Preparing','Shipped','Completed','Cancelled') DEFAULT NULL,
  `PaymentStatus` enum('Pending','Paid','PendingVerify','Refunded') DEFAULT NULL,
  `PromotionId` int DEFAULT NULL,
  `SlipImagePath` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`OrderId`),
  KEY `AddressId` (`AddressId`),
  KEY `PromotionId` (`PromotionId`),
  KEY `idx_order_status` (`Status`),
  KEY `idx_order_user` (`UserId`),
  KEY `idx_order_date` (`OrderDate`),
  CONSTRAINT `order_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`),
  CONSTRAINT `order_ibfk_2` FOREIGN KEY (`AddressId`) REFERENCES `address` (`AddressId`),
  CONSTRAINT `order_ibfk_3` FOREIGN KEY (`PromotionId`) REFERENCES `promotion` (`PromotionId`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order`
--

LOCK TABLES `order` WRITE;
/*!40000 ALTER TABLE `order` DISABLE KEYS */;
INSERT INTO `order` VALUES (16,7,5,'2026-03-20 01:43:11',80.00,'Completed','Paid',NULL,'/uploads/slips/slip_16_20260320014321.png'),(17,7,5,'2026-03-20 01:58:14',315.00,'Completed','Paid',1,'/uploads/slips/slip_17_20260320015824.jpg'),(18,7,5,'2026-03-20 02:30:15',350.00,'Completed','Paid',NULL,'/uploads/slips/slip_18_20260320023024.png'),(19,6,7,'2026-03-20 02:50:51',390.00,'Completed','Paid',NULL,'/uploads/slips/slip_19_20260320025104.png'),(20,6,7,'2026-03-20 03:08:15',108.00,'Completed','Paid',1,'/uploads/slips/slip_20_20260320030823.png'),(21,7,5,'2026-03-20 04:53:42',984.00,'Completed','Paid',3,'/uploads/slips/slip_21_20260320045351.png'),(22,6,7,'2026-03-20 23:18:32',846.00,'Completed','Paid',1,'/uploads/slips/slip_22_20260320231901.png'),(23,6,7,'2026-03-21 03:23:53',1090.00,'Completed','Paid',NULL,'/uploads/slips/slip_23_20260321032401.png'),(24,7,5,'2026-03-28 06:31:22',145.00,'Completed','Paid',4,'/uploads/slips/slip_24_20260328063131.jpg'),(25,7,5,'2026-03-28 06:34:05',470.00,'Completed','Paid',4,'/uploads/slips/slip_25_20260328063413.jpg'),(26,7,5,'2026-03-28 07:00:20',940.00,'Completed','Paid',4,'/uploads/slips/slip_26_20260328070029.jpg'),(27,7,5,'2026-03-28 18:33:20',620.00,'Completed','Paid',4,'/uploads/slips/slip_27_20260328183328.jpg'),(28,7,5,'2026-03-28 19:06:05',711.00,'Completed','Paid',1,'/uploads/slips/slip_28_20260328190623.png');
/*!40000 ALTER TABLE `order` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orderdetail`
--

DROP TABLE IF EXISTS `orderdetail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderdetail` (
  `OrderDetailId` int NOT NULL AUTO_INCREMENT,
  `OrderId` int DEFAULT NULL,
  `ProductId` int DEFAULT NULL,
  `Quantity` int DEFAULT NULL,
  `UnitPrice` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`OrderDetailId`),
  KEY `OrderId` (`OrderId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `orderdetail_ibfk_1` FOREIGN KEY (`OrderId`) REFERENCES `order` (`OrderId`),
  CONSTRAINT `orderdetail_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `stock` (`ProductId`)
) ENGINE=InnoDB AUTO_INCREMENT=57 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orderdetail`
--

LOCK TABLES `orderdetail` WRITE;
/*!40000 ALTER TABLE `orderdetail` DISABLE KEYS */;
INSERT INTO `orderdetail` VALUES (1,1,1,1,350.00),(2,1,3,1,120.00),(3,2,2,1,320.00),(4,3,5,1,85.00),(5,3,7,2,60.00),(6,4,4,1,150.00),(7,6,1,1,350.00),(8,6,2,1,320.00),(9,7,1,1,350.00),(10,7,2,1,320.00),(11,8,9,1,1.00),(12,9,9,1,1.00),(13,10,9,1,1.00),(14,11,9,1,1.00),(15,12,9,1,1.00),(16,13,9,1,1.00),(17,14,9,1,1.00),(18,15,9,1,1.00),(19,16,8,1,80.00),(20,17,1,1,350.00),(21,18,1,1,350.00),(22,20,3,1,120.00),(23,21,1,1,350.00),(24,21,2,1,320.00),(25,21,3,1,120.00),(26,21,6,1,65.00),(27,21,5,1,85.00),(28,21,4,1,150.00),(29,21,7,1,60.00),(30,21,8,1,80.00),(31,22,1,1,350.00),(32,22,2,1,320.00),(33,22,3,1,120.00),(34,22,4,1,150.00),(35,23,2,1,320.00),(36,23,1,1,350.00),(37,23,3,1,120.00),(38,23,4,1,150.00),(39,23,5,1,85.00),(40,23,6,1,65.00),(41,24,2,1,0.00),(42,25,1,1,350.00),(43,25,3,1,120.00),(44,25,2,1,0.00),(45,26,1,1,350.00),(46,26,3,1,120.00),(47,26,2,1,320.00),(48,26,4,1,150.00),(49,26,2,1,0.00),(50,27,1,1,350.00),(51,27,3,1,120.00),(52,27,4,1,150.00),(53,27,2,1,0.00),(54,28,1,1,350.00),(55,28,3,1,120.00),(56,28,2,1,320.00);
/*!40000 ALTER TABLE `orderdetail` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `promotion`
--

DROP TABLE IF EXISTS `promotion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promotion` (
  `PromotionId` int NOT NULL AUTO_INCREMENT,
  `PromotionName` varchar(100) NOT NULL,
  `Description` text,
  `DiscountValue` decimal(10,2) NOT NULL,
  `DiscountType` enum('Percent','Fixed') NOT NULL,
  `ImagePath` varchar(255) DEFAULT NULL,
  `StartDate` datetime DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  `PromoType` int NOT NULL DEFAULT '1',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `BuyQuantity` int DEFAULT '0',
  `RewardProductId` int DEFAULT NULL,
  `RewardQuantity` int DEFAULT '0',
  `IsCombinable` tinyint(1) NOT NULL DEFAULT '0',
  `RequiresProof` tinyint(1) NOT NULL DEFAULT '0',
  `MaxUsePerUser` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`PromotionId`),
  KEY `RewardProductId` (`RewardProductId`),
  CONSTRAINT `promotion_ibfk_1` FOREIGN KEY (`RewardProductId`) REFERENCES `stock` (`ProductId`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promotion`
--

LOCK TABLES `promotion` WRITE;
/*!40000 ALTER TABLE `promotion` DISABLE KEYS */;
INSERT INTO `promotion` VALUES (1,'Welcome10','ส่วนลด 10% สำหรับลูกค้าใหม่',10.00,'Percent',NULL,'2026-03-28 05:28:07','2026-04-27 05:28:07',1,1,0,NULL,0,0,0,1),(2,'SAVE50','ส่วนลด 50 บาท สำหรับออเดอร์ขั้นต่ำ 500 บาท',50.00,'Fixed',NULL,'2026-03-28 05:28:07','2026-04-27 05:28:07',1,1,0,NULL,0,0,0,1),(3,'SUMMER20','ส่วนลด 20% สำหรับฤดูร้อน',20.00,'Percent',NULL,'2026-03-28 05:28:07','2026-04-27 05:28:07',1,1,0,NULL,0,0,0,1),(4,'ถ่ายรูปเช็คอินแถมมเลย คุกกี้','ถ่ายรูปที่ร้าน พร้อมยืนยันตัว ได้คุ้กกี้สูตรอบใหม่พิเศษแถมฟรีไปเลย 1 ชิ้น',0.00,'Fixed','/uploads/promotions/promotions_20260328053240206.png','2026-03-28 05:32:00','2026-05-28 05:32:00',3,1,0,2,1,0,1,1),(5,'สมาชิกใหม่ ลด-10%','สมาชิกใหม่ ลด-10%',10.00,'Percent','/uploads/promotions/promotions_20260328055541959.png',NULL,NULL,1,1,0,NULL,0,0,0,1),(6,'สมาชิกใหม่ ลด-20%','สมาชิกใหม่ ลด-20%สมาชิกใหม่ ลด-20%',20.00,'Percent','/uploads/promotions/promotions_20260328061336089.png',NULL,NULL,1,1,0,NULL,0,0,0,1),(7,'ซื้อ 5 แถม 2','ซื้อทันทีครบ 5 รายการ แถมทันที 2 ชิ้น คุ้กกี้เนย กับเค้กวนิลา',0.00,'Fixed','/uploads/promotions/promotions_20260328071338934.png','2026-03-28 07:13:00','2026-03-28 07:13:00',2,1,5,3,2,0,0,1);
/*!40000 ALTER TABLE `promotion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `promotion_claim`
--

DROP TABLE IF EXISTS `promotion_claim`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promotion_claim` (
  `ClaimId` int NOT NULL AUTO_INCREMENT,
  `PromotionId` int NOT NULL,
  `UserId` int NOT NULL,
  `ProofImagePath` varchar(255) NOT NULL,
  `Note` text,
  `Status` enum('Pending','Approved','Rejected') NOT NULL DEFAULT 'Pending',
  `RequestedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ReviewedAt` datetime DEFAULT NULL,
  `ReviewedByUserId` int DEFAULT NULL,
  `ReviewNote` text,
  PRIMARY KEY (`ClaimId`),
  KEY `PromotionId` (`PromotionId`),
  KEY `UserId` (`UserId`),
  KEY `ReviewedByUserId` (`ReviewedByUserId`),
  CONSTRAINT `promotion_claim_ibfk_1` FOREIGN KEY (`PromotionId`) REFERENCES `promotion` (`PromotionId`),
  CONSTRAINT `promotion_claim_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promotion_claim`
--

LOCK TABLES `promotion_claim` WRITE;
/*!40000 ALTER TABLE `promotion_claim` DISABLE KEYS */;
INSERT INTO `promotion_claim` VALUES (1,4,7,'/uploads/promotion-claims/claim_7_4_20260328062956.jpg','ถ่ายแล้วฮ้ามมฟฟ','Approved','2026-03-28 06:29:56','2026-03-28 06:30:21',1,'อนุมัติโดยพนักงาน');
/*!40000 ALTER TABLE `promotion_claim` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `promotion_reward_item`
--

DROP TABLE IF EXISTS `promotion_reward_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promotion_reward_item` (
  `RewardItemId` int NOT NULL AUTO_INCREMENT,
  `PromotionId` int NOT NULL,
  `ProductId` int NOT NULL,
  `Quantity` int NOT NULL DEFAULT '1',
  `SortOrder` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`RewardItemId`),
  KEY `PromotionId` (`PromotionId`),
  KEY `ProductId` (`ProductId`),
  CONSTRAINT `promotion_reward_item_ibfk_1` FOREIGN KEY (`PromotionId`) REFERENCES `promotion` (`PromotionId`) ON DELETE CASCADE,
  CONSTRAINT `promotion_reward_item_ibfk_2` FOREIGN KEY (`ProductId`) REFERENCES `stock` (`ProductId`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `promotion_reward_item`
--

LOCK TABLES `promotion_reward_item` WRITE;
/*!40000 ALTER TABLE `promotion_reward_item` DISABLE KEYS */;
INSERT INTO `promotion_reward_item` VALUES (1,4,2,1,0),(2,7,3,2,0);
/*!40000 ALTER TABLE `promotion_reward_item` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `RoleId` int NOT NULL AUTO_INCREMENT,
  `RoleName` enum('Admin','Staff','User') NOT NULL,
  PRIMARY KEY (`RoleId`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (1,'Admin'),(2,'Staff'),(3,'User');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `stock`
--

DROP TABLE IF EXISTS `stock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stock` (
  `ProductId` int NOT NULL AUTO_INCREMENT,
  `ProductName` varchar(100) NOT NULL,
  `Description` text,
  `Price` decimal(10,2) DEFAULT NULL,
  `Stock` int DEFAULT '0',
  `CategoryId` int DEFAULT NULL,
  `ImageUrl` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ProductId`),
  KEY `CategoryId` (`CategoryId`),
  CONSTRAINT `stock_ibfk_1` FOREIGN KEY (`CategoryId`) REFERENCES `category` (`CategoryId`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stock`
--

LOCK TABLES `stock` WRITE;
/*!40000 ALTER TABLE `stock` DISABLE KEYS */;
INSERT INTO `stock` VALUES (1,'เค้กช็อคโกแลต','เค้กช็อคโกแลตเข้มข้น',350.00,11,1,'https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=400'),(2,'เค้กวานิลลา','เค้กวานิลลาหอมหวาน',320.00,6,1,'https://images.unsplash.com/photo-1464349095431-e9a21285b5f3?w=400'),(3,'คุกกี้เนย','คุกกี้เนยกรอบอร่อย',120.00,42,2,'https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=400'),(4,'คุกกี้ช็อคโกแลต','คุกกี้ช็อคโกแลตชิพ',150.00,40,2,'https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=400'),(5,'ขนมปังโฮลวีท','ขนมปังโฮลวีทเพื่อสุขภาพ',85.00,28,3,'https://images.unsplash.com/photo-1509440159596-0249088772ff?w=400'),(6,'ขนมปังกระเทียม','ขนมปังกระเทียมหอมกรอบ',65.00,38,3,'https://images.unsplash.com/photo-1549931319-a545dcf3bc7c?w=400'),(7,'ชาร้อน','ชาร้อนหอมกรุ่น',60.00,99,4,'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400'),(8,'กาแฟลาเต้','กาแฟลาเต้หอมกรุ่น',80.00,98,4,'https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400'),(9,'ชาไทยยย','ๅ',1.00,0,4,'https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400');
/*!40000 ALTER TABLE `stock` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `UserId` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `RoleId` int DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  KEY `RoleId` (`RoleId`),
  CONSTRAINT `user_ibfk_1` FOREIGN KEY (`RoleId`) REFERENCES `role` (`RoleId`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,'admin','admin','admin@bakery.com','0801234567',1),(2,'staff001','staff1234','staff@bakery.com','0812345678',2),(3,'john_doe','user1234','john@gmail.com','0823456789',3),(4,'jane_doe','user5678','jane@gmail.com','0834567890',3),(5,'Ice','user5678','ice@gmail.com','0365478963',3),(6,'thanaphat kreawring','ice123456','thanaphat.kre@spumail.net','0943253900',3),(7,'ice','ice','thanaphat.kre@spumail.net','0943253900',3);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_promotion`
--

DROP TABLE IF EXISTS `user_promotion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_promotion` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `PromotionId` int NOT NULL,
  `IsUsed` tinyint(1) DEFAULT '0',
  `AssignedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `UsedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `UserId` (`UserId`),
  KEY `PromotionId` (`PromotionId`),
  CONSTRAINT `user_promotion_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`),
  CONSTRAINT `user_promotion_ibfk_2` FOREIGN KEY (`PromotionId`) REFERENCES `promotion` (`PromotionId`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_promotion`
--

LOCK TABLES `user_promotion` WRITE;
/*!40000 ALTER TABLE `user_promotion` DISABLE KEYS */;
INSERT INTO `user_promotion` VALUES (1,7,1,1,'2026-03-20 01:22:56','2026-03-28 19:06:06'),(2,3,2,0,'2026-03-20 01:23:00',NULL),(3,3,3,0,'2026-03-20 01:24:45',NULL),(4,4,3,0,'2026-03-20 01:24:46',NULL),(5,5,3,0,'2026-03-20 01:24:46',NULL),(6,7,3,1,'2026-03-20 01:24:47','2026-03-20 04:53:42'),(7,6,1,0,'2026-03-20 01:22:56',NULL),(8,1,1,0,'2026-03-20 04:26:22',NULL),(9,2,1,0,'2026-03-20 04:26:22',NULL),(10,3,1,0,'2026-03-20 04:26:22',NULL),(11,4,1,0,'2026-03-20 04:26:22',NULL),(12,5,1,0,'2026-03-20 04:26:22',NULL),(13,6,4,0,'2026-03-28 05:33:31',NULL),(14,7,4,1,'2026-03-28 06:30:20','2026-03-28 18:33:21');
/*!40000 ALTER TABLE `user_promotion` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-29 23:15:23
