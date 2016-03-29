# SpringCard PC/SC SDK

## About this SDK

The PC/SC SDK is developed by SpringCard and is available for free to all Springcard’s customers, and helps to use with SpringCard’s PC/SC products.

## Legal disclaimer

THE SDK IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

## What is PC/SC?

PC/SC is the de-facto standard to interface Personal Computers with SmartCards.

The following document, *QuickStart for PC/SC*,  gives some information about PC/SC:

[http://www.springcard.com/en/solutions/pcsc-quickstart.html](http://www.springcard.com/en/solutions/pcsc-quickstart.html)

## What are the supported devices?

Every SpringCard PC/SC couplers may be used with this SDK.

The SDK was developed with a Prox’n’roll PC/SC HSP and a CSB-HSP Ultimate.

This page contains a complete list of SpringCard’s PC/SC couplers:

[http://www.springcard.com/en/products.html](http://www.springcard.com/en/products.html)

This other page describes how to upgrade SpringCard PC/SC couplers:

http://tech.springcard.com/firmware-upgrade/

## Introduction to SpringCardPCSC.dll

SpringCard provides a helper for Winscard.dll targeted to the .Net environment.

## Physical organisation of the SDK

The zip contains, at its root :

* A *make *folder used to compile the examples in the different languages on different platforms.

* An *src* folder, with all the library source files

* A *project* folder, with the different programs / examples with their source code.

## Mandatory tools

This SDK requires a standard C compiler and a .Net environment.

The .Net examples and projects can be used with Visual Studio 2015 Community Edition or SharpDevelop.

Both are respectively available here:

* [https://www.microsoft.com/en-us/download/details.aspx?id=48146](https://www.microsoft.com/en-us/download/details.aspx?id=48146)

* [http://www.icsharpcode.net/opensource/sd/](http://www.icsharpcode.net/opensource/sd/)

To work with the beginners examples (see below), a MIFARE Ultralight card, a MIFARE Classic card and a MIFARE DESFire card are required.

Please contact our sales team, should  you need to purchase some cards :

 [sales@springcard.com](mailto:sales@springcard.com)

## The beginners

The *project/beginners *folder contains a serie of very basic examples in C# and in Visual Basic.

There is a solution with 9 projects:

* SpringCardPCSC: It’s a shared project with all the other projects and it contains the source of our PC/SC helper.

* listReaders: It’s the most basic example, it lists all the PC/SC readers installed on  the computer. That’s the project to start with.

* getCardUid: This program builded on the listReaders example, retrieves the readers but also retrieves some data from the inserted card as the ATR, UID, protocol and type.

* readMifareUltralight: This project reads all the content of a Mifare Ultralight card.

* writeMifareUltralight: This project writes some ASCII (text) data to a Mifare Ultralight card. It only write textual data but  it’s not a limitation of the card.

* readMifareClassic: Reads data (only textual data) from a Mifare Classic card.

* writeMifareClassic: Writes (textual) data in the card to a specific address.

* desfireInformation: Retrieves information from a desFire card. It reads the card’s version and lists the available applications.

* getReaderInformation: This project get some information from the reader:

    * Vendor's Name

    * Product's name

    * Product's Serial number

    * USB vendor ID and product ID

    * Product's version

    * etc

This program differs from the preceding ones, in that it shows how to communicate with the reader instead of communicating with the card.

Please note that when dealing with PC/SC, it is highly recommended to use threads, in order not to block the user interface. It is also mandatory to handle all possible exceptions.

All those examples don’t necessary use threads nor handle exceptions because the goal is to demonstrate how to deal with PC/SC, cards and readers.

## General principle

Using SpringCardPCSC.dll to talk to a card is straightforward:

1. Create a reader object with the reader’s name (as returned by Windows)

2. Start to monitor it (whenever something happens, a callback is called)

3. In the callback it is possible to check for the reader’s state and the card’s ATR

4. If the card’s ATR is not null an SCardChannel object can be created to talk to the card and to send it some APDUs

5. Each command sent to the card returns a response that must be interpreted.

It is highly recommended to use some threads to deal with the reader and it is highly recommended to handle PC/SC errors and .Net exceptions.

## Other tools

### memorycardtool

This Windows project is used to "explore" memory cards (show data and modify them). It shows data in raw mode.

### pcscdiag2

This Windows program is used to send commands to memory cards and smartcards.

### scriptorxv

This is a Windows program to send commands to a smartcard -from a batch file or manual entry- through a PC/SC reader.

### pcsc_no_minidriver

Command line utility to disable the 'driver not found' message on Windows Seven for smartcards.

### c_reference

The folder contains some examples in the C language to talk with cards.

### pcscmon

This sample program shows how to use SCardGetStatusChange to track smartcard events.

### scard_on_mcu

Implementation of CCID over Serial for MCU targets.

## Other readings

The "*CSB6 Developer's reference manual*" is a good reading to understand what SpringCard readers can do and how to use the different cards:

http://www.springcard.com/en/download/find/file/pmd841p

## How to contact us

For technical information, this form is available:

[http://www.springcard.com/en/support/contact](http://www.springcard.com/en/support/contact)

For any commercial request, please use this other form:

[http://www.springcard.com/en/contact](http://www.springcard.com/en/contact)

