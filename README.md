<p align="center"><img src="assets/occhio.png" width="120" alt="Occhio di Lince"></p>

<h1 align="center">Occhio di Lince</h1>
<p align="center">Strumento di diagnostica PC usato dallo staff per i controlli anti-cheat su FiveM.</p>

---

## Da dove scaricarlo

**Solo dalla sezione [Releases](../../releases/latest) di questo repository.** Lo staff ti manderà il link a questa pagina durante il controllo. Non usare copie ricevute in altri modi.

Usalo **solo quando uno staffer te lo chiede**: senza il codice monouso che ti dà lo staff, il programma non fa nulla.

## Cosa fa, passo per passo

1. **Chiede i permessi di amministratore a Windows.** Servono per leggere alcune informazioni di sistema (per esempio i programmi avviati di recente). Se rifiuti, il programma si chiude senza fare nulla.
2. **Ti chiede il codice** che ti ha dato lo staffer. Il codice vale una sola volta e scade dopo 15 minuti.
3. **Ti mostra chi ha richiesto il controllo e cosa verrà letto**, e ti chiede di accettare (`S`) o rifiutare (`N`). Se rifiuti, lo staff viene informato del rifiuto.
4. **Se accetti**, scarica dal server dello staff il modulo di raccolta valido solo per quel controllo, lo esegue e invia i dati in modo protetto (HTTPS).
5. Alla fine vedi solo *"Dati inviati allo staff"*. Il risultato lo vede solo lo staff.

Tutto il codice del programma è in [`src/Agent.cs`](src/Agent.cs): è breve e puoi leggerlo.

## Cosa viene letto

- programmi avviati di recente e processi in esecuzione
- cartelle di FiveM e GTA V, file scaricati, cestino, file recenti
- servizi e registri di Windows, Windows Defender, dispositivi USB
- cronologia del browser e cache DNS, **solo le voci legate ai cheat**
- download di programmi e archivi degli ultimi 30 giorni
- programmi ad avvio automatico e cartelle aperte di recente
- un'impronta hardware anonima (solo hash, nessun numero di serie in chiaro)

L'impronta (hash) dei file sospetti può essere verificata su VirusTotal e MalwareBazaar: **i tuoi file non vengono mai inviati**.

## Cosa NON fa

- non modifica, non cancella e non installa nulla
- non resta attivo dopo il controllo e non si avvia da solo
- non legge password, chat, foto o il contenuto dei tuoi documenti
- non invia nulla senza il tuo consenso esplicito

I dati raccolti sono visibili solo allo staff addetto ai controlli e vengono cancellati dopo 60 giorni. Il report con l'esito resta.

## Perché il modulo di raccolta non è pubblico

Il programma scarica il modulo di raccolta dal server dello staff solo dopo che hai inserito un codice valido. Non è pubblicato qui perché contiene l'elenco preciso delle tracce che controlliamo: renderlo pubblico darebbe a chi bara una lista di cosa nascondere. Il modulo **legge soltanto**, e prima di partire il programma ti mostra cosa viene letto.

## Verifica del file

Ogni versione è compilata **automaticamente da GitHub** a partire dal codice di questo repository: nessuno la compila a mano. Puoi verificarlo in tre modi.

1. **Impronta SHA256.** Nella pagina della release trovi l'impronta del file. Per controllarla, apri PowerShell nella cartella dove hai scaricato il file:
   ```powershell
   Get-FileHash .\Occhio.exe -Algorithm SHA256
   ```
   Il valore deve essere identico a quello della release.
2. **Attestazione di GitHub.** Ogni file ha un'attestazione firmata da GitHub che lo collega al codice e alla compilazione da cui è nato. Con la [GitHub CLI](https://cli.github.com/):
   ```powershell
   gh attestation verify .\Occhio.exe --repo GiGiProjects/occhio-di-lince
   ```
3. **Scansione antivirus.** Ogni release ha il link alla scansione su [VirusTotal](https://www.virustotal.com). I principali antivirus (Microsoft Defender, Kaspersky, ESET, Bitdefender, Malwarebytes e altri) lo considerano pulito. Qualche motore minore può segnalarlo con nomi generici: succede spesso ai programmi nuovi senza firma digitale che chiedono i permessi di amministratore.

## Avviso di Windows

Al primo avvio Windows può mostrare *"Windows ha protetto il PC"*. Succede perché il programma non ha una firma digitale a pagamento. Clicca **Ulteriori informazioni → Esegui comunque**. Anche Discord mostra un avviso quando scarichi un file `.exe`.

## Domande o problemi

Rivolgiti allo staff tramite il sistema di ticket del server.

---

<sub>Il codice è pubblicato per trasparenza. Tutti i diritti riservati: non è consentito riutilizzarlo, modificarlo o ridistribuirlo senza autorizzazione.</sub>
