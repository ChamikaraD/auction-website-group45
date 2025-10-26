# auction-website-group45
# NumisLive - Online Coin Auction System 🪙

## Description

NumisLive is a web application built with **ASP.NET Core MVC** and **C#** designed specifically for coin collectors (numismatists). It provides a dedicated platform for users to list, browse, bid on, and purchase rare and valuable coins in a secure online auction environment. This project evolved from a basic antique auction system into a feature-rich platform tailored to the needs of the numismatic community.

---

## Key Features ✨

* **User Authentication:** Secure registration and login (including Google Sign-in) using ASP.NET Core Identity.
* **Coin-Specific Listings:** Users can create detailed auction listings with fields like Year, Grade, Country, Denomination, Mint Mark, Composition, and image uploads.
* **Live Bidding:** Real-time auction countdown timers and bidding activity updates using SignalR.
* **Secure Payments:** Integration with Stripe Checkout for processing payments for won auctions.
* **Notifications:**
    * **Email Notifications:** Automatic emails (via SendGrid) for events like winning an auction or being outbid.
    * **In-System Notifications:** Real-time on-page alerts (via SignalR) for bidding updates.
* **User Management:** Dedicated pages for users to manage their listings (`My Listings`), bids (`My Bids`), and account settings.
* **Admin Panel:** A restricted area for administrators to manage users (activate/deactivate/delete), view all listings, manage bids, and view payment records.
* **Browse & Search:** Users can browse active auctions or all listings and search for specific coins.

---

## Technologies Used 🛠️

* **Backend:** C#, ASP.NET Core MVC (.NET 8 - *Adjust version if needed*)
* **Database:** Entity Framework Core, SQL Server
* **Authentication:** ASP.NET Core Identity
* **Real-time:** SignalR
* **Payments:** Stripe.net, Stripe Checkout
* **Email:** SendGrid
* **Frontend:** Razor Views, HTML, CSS, JavaScript, Bootstrap (potentially customized)
* **IDE:** Visual Studio 2022 (*Adjust if needed*)

---

## Setup & Installation 🚀

1.  **Clone the Repository:**
    ```bash
    git clone [https://github.com/your-username/your-repository-name.git](https://github.com/your-username/your-repository-name.git)
    cd your-repository-name
    ```
2.  **Configure Secrets:**
    * Right-click the project in Visual Studio -> **Manage User Secrets**.
    * Add your configuration keys to the `secrets.json` file:
        ```json
        {
          "ConnectionStrings": {
            "DefaultConnection": "Your_SQL_Server_Connection_String"
          },
          "Authentication:Google:ClientId": "YOUR_GOOGLE_CLIENT_ID",
          "Authentication:Google:ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
          "Stripe": {
            "SecretKey": "YOUR_STRIPE_SECRET_KEY",
            "PublishableKey": "YOUR_STRIPE_PUBLISHABLE_KEY" // Optional: Add if needed client-side
          },
          "SendGridKey": "YOUR_SENDGRID_API_KEY"
        }
        ```
    * Replace placeholder values with your actual keys and connection string.
3.  **Update Database:**
    * Open the Package Manager Console (**Tools -> NuGet Package Manager -> Package Manager Console**).
    * Run the command: `Update-Database` (This applies existing migrations).
4.  **Configure SendGrid Sender:**
    * Ensure the email address used in `Services/EmailSender.cs` (`var from = new EmailAddress(...)`) is verified in your SendGrid account.
5.  **Run the Application:** Press `F5` or click the Run button in Visual Studio.

---

## Default Admin Credentials 🔑

Upon first run, the application seeds a default administrator account:

* **Username/Email:** `admin@example.com`
* **Password:** `Admin@123`

Log in with these credentials to access the **ADMIN** section.

---

## Contribution 🤝

This project was developed collaboratively by a team of 8 members as part of [Mention Course/Project Context if applicable]. Each member contributed to different aspects of the frontend and backend development.

*(Optional: You can add a brief list of team members here if desired)*
