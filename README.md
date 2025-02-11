![image](https://user-images.githubusercontent.com/82561200/222894826-8f168c78-477a-428e-9af5-7076851f5ceb.png)
# DemoForum
---

### 簡介

一個 ASP.NET Core MVC 做的論壇專案。
因主要開發項目為後端，前端介面為簡單的 CSS + cshtml。

---

### Demo 網頁

可以在[這裡試玩看看](https://demo-forum.teamol-developing.net)。

---

### 展示內容

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
    - Entity Framework Core
    - LINQ 語法實現 Repositories
5. CI/CD
    - （僅第一次）透過與專案共同提供的 Docker-Compose 建置
    - 透過 GitHub Runner 執行包 Docker Image, 推上 Docker Hub
    - 伺服器透過 Watchtower 自動檢查更新
6. 網路架構
    - 利用 Cloudflare Tunnel 對外開放，避免另外需要開防火牆，同時保持了設定彈性

---

### 一些截圖
![image](https://user-images.githubusercontent.com/82561200/222894835-32222a0b-4c77-460d-b1a4-d3139ccd79d0.png)
![image](https://user-images.githubusercontent.com/82561200/222894845-e3ded71b-d14f-41f4-88f5-436859b79c78.png)

---

### 課題

1. ~~完成 CI/CD 流程~~
2. ~~完成推文數顯示~~
3. ~~完成內文超連結 href~~
4. 完成修改推文功能
5. 完成使用者暱稱、頭像設定
6. 補足註解
