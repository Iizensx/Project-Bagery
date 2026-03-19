ALTER TABLE `order`
MODIFY COLUMN `Status` ENUM('Pending','Paid','Preparing','Shipped','Completed','Cancelled') NULL;
