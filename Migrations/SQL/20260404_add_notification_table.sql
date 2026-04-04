CREATE TABLE `notification` (
    `NotificationId` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Type` varchar(50) NOT NULL,
    `Title` varchar(150) NOT NULL,
    `Message` text NULL,
    `LinkUrl` varchar(255) NULL,
    `ReferenceType` varchar(50) NULL,
    `ReferenceId` int NULL,
    `IsRead` tinyint(1) NOT NULL DEFAULT '0',
    `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`NotificationId`),
    KEY `UserId` (`UserId`),
    CONSTRAINT `notification_ibfk_1`
        FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
        ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_0900_ai_ci;
