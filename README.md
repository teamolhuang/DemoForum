# DemoForum
---

### Demo 連結

http://ec2-18-180-93-247.ap-northeast-1.compute.amazonaws.com/

---

### 簡介

---

一個 ASP.NET Core MVC 做的論壇專案。開發用的 DB 為 MS SQL Server。
因主要開發項目為後端，前端介面為簡單的 CSS + cshtml。

---

### 展示內容

---

1. CRUD
  - 註冊與登入
  - 發表、修改、刪除文章
  - 推文
2. 使用者驗證
  - Cookie 驗證，配合 ASP.NET Core 提供的 AntiForgeryToken
3. 單元測試
  - NUnit
  - Moq
  - In-Memory DB
4. ORM
  - Entity Framework Core (EF7)
  - LINQ 語法實現 Repositories

# AWS
1. 程式專案、DB 在本地開發並測試。
2. 開發完後，各自打包成 image。
3. 撰寫 Docker-Compose。
4. 在 AWS 上建置 EC2、ECR，並利用 IAM 讓 Docker 能順利登入。
5. ECR 建置私有庫，在 EC2 實例上 pull 之後執行 Docker-Compose。
