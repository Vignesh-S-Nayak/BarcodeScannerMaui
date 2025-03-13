# Barcode Scanner - MAUI App

A cross-platform mobile application for scanning, logging, and managing barcodes built with .NET MAUI.

## Overview

The app allows users to scan product barcodes using their device's camera, save the scanned items in a local database, and manage their inventory. This app is built using .NET MAUI (Multi-platform App UI) framework, which enables it to run natively on Android, iOS, Windows, and macOS from a single codebase.

## Usage

1. **Login**: Use the provided credentials [username ="admin" and password ="password"]
2. **Scan Barcodes**: Press "Start Scanning" to open the camera
3. **Position the Barcode**: Align the barcode within the red square
4. **View Items**: Switch to the "Items" tab to view scanned items
5. **Manage Items**: Remove individual items or clear all items

## Architecture

The application follows the MVVM (Model-View-ViewModel) pattern:

- **Models**: Data structures representing item information
- **Views**: XAML UI components for presenting data and capturing user input
- **ViewModels**: Classes that handle UI logic and communicate with services
- **Services**: Business logic for authentication and data storage

## Key Components

- **CameraPage**: Handles barcode scanning using device camera
- **ItemsPage**: Displays and manages scanned items
- **LoginPage**: Handles user authentication
- **ItemService**: Manages item data operations
- **AuthService**: Handles authentication logic

## Screenshots

| Login Page | Scanner Page | Empty Cart | Items Page |
|------------|-------------|------------|------------|
| ![Login Page](https://github.com/user-attachments/assets/0ff12f0e-eef9-4e30-abe5-52967b028899) | ![Scanner Page](https://github.com/user-attachments/assets/5832797b-4d49-4539-af1a-5618bd067087) | ![Empty Cart](https://github.com/user-attachments/assets/2816c7df-d5de-4944-ac77-9dc905f973c9) | ![Items Page](https://github.com/user-attachments/assets/93679b64-749c-4e48-b4f5-454d3aee547f) |


