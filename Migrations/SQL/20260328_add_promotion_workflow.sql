ALTER TABLE `promotion`
    ADD COLUMN `ImagePath` varchar(255) NULL AFTER `DiscountType`,
    ADD COLUMN `StartDate` datetime NULL AFTER `ImagePath`,
    ADD COLUMN `EndDate` datetime NULL AFTER `StartDate`,
    ADD COLUMN `PromoType` int NOT NULL DEFAULT 1 AFTER `EndDate`,
    ADD COLUMN `IsActive` tinyint(1) NOT NULL DEFAULT 1 AFTER `PromoType`,
    ADD COLUMN `BuyQuantity` int NULL DEFAULT 0 AFTER `IsActive`,
    ADD COLUMN `RewardProductId` int NULL AFTER `BuyQuantity`,
    ADD COLUMN `RewardQuantity` int NULL DEFAULT 0 AFTER `RewardProductId`,
    ADD COLUMN `IsCombinable` tinyint(1) NOT NULL DEFAULT 0 AFTER `RewardQuantity`,
    ADD COLUMN `RequiresProof` tinyint(1) NOT NULL DEFAULT 0 AFTER `IsCombinable`,
    ADD COLUMN `MaxUsePerUser` int NOT NULL DEFAULT 1 AFTER `RequiresProof`,
    ADD KEY `RewardProductId` (`RewardProductId`),
    ADD CONSTRAINT `promotion_ibfk_1`
        FOREIGN KEY (`RewardProductId`) REFERENCES `stock` (`ProductId`);

CREATE TABLE `promotion_claim` (
    `ClaimId` int NOT NULL AUTO_INCREMENT,
    `PromotionId` int NOT NULL,
    `UserId` int NOT NULL,
    `ProofImagePath` varchar(255) NOT NULL,
    `Note` text NULL,
    `Status` enum('Pending','Approved','Rejected') NOT NULL DEFAULT 'Pending',
    `RequestedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ReviewedAt` datetime NULL,
    `ReviewedByUserId` int NULL,
    `ReviewNote` text NULL,
    PRIMARY KEY (`ClaimId`),
    KEY `PromotionId` (`PromotionId`),
    KEY `UserId` (`UserId`),
    KEY `ReviewedByUserId` (`ReviewedByUserId`),
    CONSTRAINT `promotion_claim_ibfk_1`
        FOREIGN KEY (`PromotionId`) REFERENCES `promotion` (`PromotionId`),
    CONSTRAINT `promotion_claim_ibfk_2`
        FOREIGN KEY (`UserId`) REFERENCES `user` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

UPDATE `promotion`
SET
    `StartDate` = COALESCE(`StartDate`, NOW()),
    `EndDate` = COALESCE(`EndDate`, DATE_ADD(NOW(), INTERVAL 30 DAY)),
    `PromoType` = COALESCE(NULLIF(`PromoType`, 0), 1),
    `IsActive` = 1,
    `BuyQuantity` = COALESCE(`BuyQuantity`, 0),
    `RewardQuantity` = COALESCE(`RewardQuantity`, 0),
    `IsCombinable` = COALESCE(`IsCombinable`, 0),
    `RequiresProof` = COALESCE(`RequiresProof`, 0),
    `MaxUsePerUser` = COALESCE(NULLIF(`MaxUsePerUser`, 0), 1);
