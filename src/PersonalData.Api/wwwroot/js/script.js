const form = document.querySelector('#generateForm');
const endpoint = document.querySelector('#endpoint');
const count = document.querySelector('#count');
const countField = document.querySelector('#countField');
const button = document.querySelector('#generateButton');
const output = document.querySelector('#output');
const status = document.querySelector('#status');
const error = document.querySelector('#error');

endpoint.addEventListener('change', () => {
    const multiplePersons = endpoint.value === 'persons';
    countField.hidden = !multiplePersons;
    count.disabled = !multiplePersons;
});

form.addEventListener('submit', async (event) => {
    event.preventDefault();

    let url = `/api/${endpoint.value}`;
    if (endpoint.value === 'persons') {
        url += `?count=${count.value}`;
    }

    button.disabled = true;
    button.textContent = 'Genererer…';
    output.setAttribute('aria-busy', 'true');
    output.replaceChildren();
    error.hidden = true;
    status.textContent = 'Henter data…';

    try {
        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`API’et svarede med HTTP ${response.status}. Prøv igen, og kontrollér at databasen kører.`);
        }

        const data = await response.json();
        const persons = Array.isArray(data) ? data : [data];
        persons.forEach((person, index) => showCard(person, index));
        status.textContent = persons.length === 1 ? '1 resultat genereret' : `${persons.length} personer genereret`;
    } catch (exception) {
        error.textContent = exception instanceof TypeError
            ? 'Kunne ikke kontakte API’et. Kontrollér forbindelsen, og prøv igen.'
            : exception.message;
        error.hidden = false;
        status.textContent = 'Kunne ikke hente data';
    } finally {
        button.disabled = false;
        button.textContent = 'Generér data';
        output.setAttribute('aria-busy', 'false');
    }
});

function showCard(person, index) {
    const card = document.createElement('article');
    card.className = 'card';

    const heading = document.createElement('h3');
    heading.textContent = person.firstName
        ? `${person.firstName} ${person.lastName}`
        : `Resultat ${index + 1}`;
    card.appendChild(heading);

    const details = document.createElement('dl');
    addRow(details, 'CPR', person.cpr);
    addRow(details, 'Fornavn', person.firstName);
    addRow(details, 'Efternavn', person.lastName);
    addRow(details, 'Køn', person.gender === 'female' ? 'Kvinde' : person.gender === 'male' ? 'Mand' : person.gender);
    addRow(details, 'Fødselsdato', person.birthDate);

    // /api/address returns the address directly; /api/person nests it.
    const address = person.address ?? (person.street !== undefined ? person : null);
    if (address) {
        addRow(details, 'Adresse', `${address.street} ${address.number}, ${address.floor}. ${address.door}`);
        addRow(details, 'Postnr. og by', `${address.postalCode} ${address.townName}`);
    }

    addRow(details, 'Telefon', person.phoneNumber);
    card.appendChild(details);
    output.appendChild(card);
}

function addRow(details, label, value) {
    if (value === undefined || value === null) return;

    const row = document.createElement('div');
    row.className = 'row';
    const term = document.createElement('dt');
    term.textContent = label;
    const description = document.createElement('dd');
    description.textContent = value;
    row.append(term, description);
    details.appendChild(row);
}
