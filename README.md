# 🎉 BirthdayBot

A simple Discord bot built with C# and Discord.Net that remembers users' birthdays, announces them in a chosen channel, and allows users to view or remove their birthday.

---

## 📦 Features

- `/birthday MM-DD` — Save your birthday (e.g., `/birthday 04-03`)
- `/mybirthday` — Check your saved birthday
- `/removebirthday` — Delete your birthday from the list
- 🎂 Automatically announces birthdays at 9:00 AM every day in a specified Discord channel

---

## ⚙️ Setup

### 1. Clone the repository
```bash
git clone https://github.com/EvinCB/Birthday-Bot.git
cd Birthday-Bot


### 2. Install dependencies

Make sure you have:
- .NET 6 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Discord.Net (`Discord.Net.WebSocket`) via NuGet
- DotNetEnv (`DotNetEnv`) via NuGet

### 3. Create a `.env` file

Inside the project root, create a file named `.env` and add the following:

DISCORD_TOKEN=your_bot_token_here            //type exaclty like this. no spaces in front or behind the =
BIRTHDAY_CHANNEL_ID=your_channel_id_here


❗ Do **not** share your bot token — keep this file private.

### 4. Run the bot

From the terminal or Visual Studio:

Or compile and run the `.exe` manually from the `bin/Debug/net6.0` folder.

---

## 🖥️ Run at Startup (Optional)

To run the bot automatically on system boot using Task Scheduler:

- Use the `.exe` located in `bin/Debug/net6.0/BirthdayBot.exe`
- Set the **Start in** field in Task Scheduler to the same folder so it can find `.env`

---

## 📁 File Structure

BirthdayBot/ │ ├── Program.cs # Main bot logic ├── birthdays.json # Stores user birthdays ├── .env # Stores your private bot token and channel ID (ignored by Git) ├── .gitignore # Keeps sensitive and build files out of Git └── README.md # You're reading it!


---

## 🙅‍♂️ GitHub Safety

✅ `.env` and `birthdays.json` are listed in `.gitignore` so they don’t get pushed to GitHub.

---

## 📢 License

This project is for learning and demonstration purposes. You are free to modify and expand it.

---

Made with ☕ and C# by [Evin B]


