CREATE TABLE `historyorder` (
  `HistoryOrderId` INT NOT NULL AUTO_INCREMENT,
  `OrderId` INT NULL,
  `UserId` INT NULL,
  `OrderDate` DATETIME NULL,
  `CompletedAt` DATETIME NULL,
  `TotalAmount` DECIMAL(10,2) NULL,
  `Status` VARCHAR(50) NULL,
  `PaymentStatus` VARCHAR(50) NULL,
  `DeliveryAddress` VARCHAR(500) NULL,
  `ItemSummary` TEXT NULL,
  PRIMARY KEY (`HistoryOrderId`),
  KEY `OrderId` (`OrderId`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `historyorder_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
