ALTER TABLE `user`
    ADD COLUMN `OtpCode` varchar(10) NULL AFTER `RoleId`,
    ADD COLUMN `OtpExpiredAt` datetime NULL AFTER `OtpCode`;
