# Black-box test design: Personal Test Data Generator

First Mandatory Assignment, Software Quality E2026

## 1. Approach

The test cases below come from the assignment's requirements only. Each section quotes the requirement it is derived from. Where the requirements are silent, the decision is written down as an assumption (A1, A2, …, see section 9) so that it can be tested.

Techniques used:

- **Equivalence partitioning (EP).** Every input and output is split into valid and invalid partitions, and one representative value is picked from each.
- **3-value boundary value analysis (BVA).** For every boundary *b*, the values *b − 1*, *b* and *b + 1* are tested. The step is the smallest unit of the value: 1 for integers, 1 day for dates, 1 character for lengths and character codes.
- **Decision tables** where the result depends on a combination of conditions: gender and CPR check digit, and cross-field consistency per endpoint.
- **State transition testing is not used.** Every endpoint is a stateless `GET`; no request changes what a later request returns.

A data generator has few inputs the caller controls. Most of what needs testing is the *output*. The tables are therefore used in three ways:

1. **Real inputs** are tested directly: `count` for bulk persons, `birthDate` and `gender` for the CPR generator, "today" (injected `TimeProvider`) for the date of birth, and the size of the source data (names file, postal code table, through stub repositories).
2. **Output oracles.** Each output rule (CPR format, door format, phone prefix, …) is implemented once as a validator in the test project. The validator is tested with *every* value in the tables, valid and invalid, which proves it can actually reject bad data.
3. **Generator sampling.** Each generator produces 1,000 values from a fixed-seed `Random`; every value must pass its validator. Where the requirements list several valid forms (e.g. the three door formats), each form must occur at least once. The fixed seed makes the run deterministic.

| Section | Test level |
|-|-|
| 2 Bulk count | Unit (service), API (Postman) |
| 3 Name and gender | Unit (stub repository), integration (real `person-names.json`) |
| 4 Date of birth | Unit (fixed `TimeProvider`) |
| 5 CPR | Unit |
| 6 Address | Unit (stub repository), integration (MySQL) |
| 7 Mobile phone | Unit |
| 8 Cross-field consistency | Unit (service), API (Postman) |

Notation: ✔ valid, ✘ invalid. MIN / MAX are `int.MinValue` / `int.MaxValue`. Test dates assume "today" is fixed to **2026-11-09**.

## 2. Bulk count: `GET /api/persons?count=N`

> Return fake person information in bulk (all information for 2 to 100 persons)

| Partition type | Partition | EP value | Expected | Boundary values | BVA test values |
|-|-:|-:|-|-:|-:|
| Invalid | MIN to −1 | −50 | 400 (A1) | −1 | −2 −1 0 |
| Invalid | 0 | 0 | 400 | 0 | −1 0 1 |
| Invalid | 1 | 1 | 400 | 1 | 0 1 2 |
| Valid | 2 to 100 | 51 | 200, exactly *N* persons | 2, 100 | 1 2 3 99 100 101 |
| Invalid | 101 to MAX | 500 | 400 | 101 | 100 101 102 |

`count` arrives as text in the query string, so the API has further invalid partitions outside the integer domain:

| Partition type | Partition | Test value | Expected |
|-|-|-|-|
| Invalid | Parameter missing | `/api/persons` | 400 |
| Invalid | Empty | `count=` | 400 |
| Invalid | Not a number | `count=abc` | 400 |
| Invalid | Decimal | `count=2.5` | 400 |
| Invalid | Larger than `int` | `count=2147483648` | 400 |

**Final list:** MIN, −50, −2, −1, 0, 1, 2, 3, 51, 99, 100, 101, 102, 500, MAX, plus the five text values above. For every valid count, each returned person must also pass sections 3 to 8.

## 3. Name and gender

> First name, Last name, Gender: randomly extracted from the provided file person-names.json

| Field | Partition type | Partition | Test value |
|-|-|-|-|
| Gender | Valid | `female` | `female` |
| | Valid | `male` | `male` |
| | Invalid | Other casing | `Female` |
| | Invalid | Other value | `other` |
| | Invalid | Empty | `""` |
| Record | Valid | First name, last name and gender are one record of the file (A2) | `Annemette P.`, `Nilsson`, `female` |
| | Invalid | Gender taken from another record | `Annemette P.`, `Nilsson`, `male` |
| | Invalid | Name not in the file | `Test`, `Testesen`, `female` |
| | Invalid | Empty first or last name | `""`, `Nilsson`, `female` |

The file contains first names with spaces and periods (`Annemette P.`), so the oracle compares against the file and does not require letters only.

**BVA on the size of the source data** (stub `IPersonNameRepository`):

| Records in source | Expected |
|-|-|
| 0 | Error (A3) |
| 1 | Always that record |
| 2 (one female, one male) | Both records returned within 1,000 seeded draws. This catches an off-by-one random index that never picks the last record |

With the real file (536 records: 251 female, 285 male), every result must be a record of the file and both genders must occur.

## 4. Date of birth

> Date of birth. Must match the date in the CPR

The requirement sets no range. The generator interface promises "never in the future". The lower bound is assumption A4: the date lies within the last 100 years, **(today − 100 years, today]**. The reason: the CPR stores the year as two digits, so only a window shorter than 100 years keeps `ddMMyy` unambiguous. 1926-11-10 becomes `101126`, and so would 2026-11-10; the window keeps only one of them valid.

| Partition type | Partition | EP value | Boundary values | BVA test values |
|-|-|-:|-:|-|
| Invalid | On or before 1926-11-09 | 1900-01-01 | 1926-11-09 | 1926-11-08 ✘, 1926-11-09 ✘, 1926-11-10 ✔ |
| Valid | 1926-11-10 to 2026-11-09 | 1976-06-15 | 1926-11-10, 2026-11-09 | 1926-11-09 ✘, 1926-11-10 ✔, 1926-11-11 ✔, 2026-11-08 ✔, 2026-11-09 ✔, 2026-11-10 ✘ |
| Invalid | After 2026-11-09 | 2030-01-01 | 2026-11-10 | 2026-11-09 ✔, 2026-11-10 ✘, 2026-11-11 ✘ |

The boundaries move with "today". The lower BVA is repeated with today = **2028-02-29** (leap day). 100 years earlier, 1928-02-29 also exists, so 1928-02-29 ✘ and 1928-03-01 ✔. This checks that the generator reads the injected `TimeProvider` and not the system clock.

In the API, the date is returned as ISO `yyyy-MM-dd` (A5): `1976-06-15` ✔, `15-06-1976` ✘, `1976-6-15` ✘, `1976-02-30` ✘ (no such date).

## 5. CPR: `Generate(birthDate, gender)`

> CPR. 10 numeric digits. The first six ones identify the date of birth in the format ddMMyy. The last four digits are randomly generated, with the following constraint: For female persons, the final number is even. For male persons, the final number is odd

### 5.1 Output format

| Aspect | Partition type | Partition | Test value |
|-|-|-|-|
| Length | Invalid | 0 characters | `""` |
| | Invalid | 9 digits | `010100123` |
| | Valid | 10 digits | `0101001234` |
| | Invalid | 11 digits | `01010012345` |
| Characters | Valid | Digits only | `0101001234` |
| | Invalid | Contains a letter | `010100123a` |
| | Invalid | Dash (common written form) | `010100-1234` |
| | Invalid | Space | `010100 1234` |
| Date part | Valid | First six = `birthDate` as `ddMMyy` | 2000-01-01 → `010100…` |
| | Invalid | Month and day swapped (`MMddyy`) | 2000-01-31 → `013100…` |
| | Invalid | ISO order (`yyMMdd`) | 2000-01-31 → `000131…` |

When the CPR is returned without a date of birth (`/api/cpr`), the first six digits must still form a real date inside the window from section 4:

| Part | BVA test values |
|-|-|
| Day `dd` | `00` ✘, `01` ✔, `31` ✔ (in a 31-day month), `32` ✘ |
| Month `MM` | `00` ✘, `01` ✔, `12` ✔, `13` ✘ |
| Calendar | `290224` ✔ (2024 is a leap year), `290223` ✘ (2023 is not), `310487` ✘ (April has 30 days) |

### 5.2 Date part: input `birthDate`

The partitions follow how `ddMMyy` is written: one-digit values need a leading zero, and the year keeps only its last two digits.

| Component | Partition | Boundary values |
|-|-|-|
| Day | 1–9 (leading zero) | 1, 9 |
| | 10 to last day of month | 10, 28 / 29 / 30 / 31 |
| Month | 1–9 (leading zero) | 1, 9 |
| | 10–12 | 10, 12 |
| Year, last two digits | 00 | 2000 |
| | 01–09 (leading zero) | 2001, 2009 |
| | 10–99 | 2010, 1999 |

Test cases, combined so that each covers several boundaries:

| `birthDate` | Expected first six | Covers |
|-|-|-|
| 2000-01-01 | `010100` | Day 1, month 1, year 00 |
| 1999-12-31 | `311299` | Day 31, month 12, year 99 |
| 2001-09-09 | `090901` | Day 9, month 9, year 01 |
| 2009-10-10 | `101009` | Day 10, month 10, year 09 |
| 2010-02-28 | `280210` | End of February (non-leap), year 10 |
| 2024-02-29 | `290224` | Leap day |
| 1987-04-30 | `300487` | End of a 30-day month |
| 1926-11-10 | `101126` | Oldest valid date of birth (section 4) |
| 2026-11-09 | `091126` | Today |

### 5.3 Gender and final digit: decision table

| Condition / action | R1 | R2 | R3 | R4 |
|-|-|-|-|-|
| Gender | female | female | male | male |
| Final digit | even | odd | odd | even |
| **Result** | ✔ | ✘ | ✔ | ✘ |

BVA on the digit range 0–9 within each parity:

| Gender | ✔ boundary digits | ✘ boundary digits |
|-|-|-|
| female | 0, 8 | 1, 9 |
| male | 1, 9 | 0, 8 |

Sampling: 1,000 CPRs per gender. Every final digit has the right parity, and all five allowed digits occur. This catches a random range that leaves out 0, or one that stops at 8 instead of 9.

Invalid input: an undefined `Gender` value, such as `(Gender)2`, raises `ArgumentOutOfRangeException` (A6).

## 6. Address

### 6.1 Street

> Street. A random assortment of alphabetic characters

| Partition type | Partition | Test value |
|-|-|-|
| Valid | Letters only (A7) | `Kongevej` |
| Valid | Danish letters (A7) | `Østergade` |
| Valid | Space after the first letter (A7) | `Gammel Kongevej` |
| Invalid | Empty | `""` |
| Invalid | Starts with a space | ` Kongevej` |
| Invalid | Digit | `Vej12` |
| Invalid | Punctuation | `Sankt.Hans` |

| BVA | Test values |
|-|-|
| Length | 0 ✘, 1 ✔, 2 ✔ |
| Uppercase range (ASCII neighbours) | `@` ✘, `A` ✔, `Z` ✔, `[` ✘ |
| Lowercase range (ASCII neighbours) | `` ` `` ✘, `a` ✔, `z` ✔, `{` ✘ |

### 6.2 Number

> Number. A number from 1 to 999 optionally followed by an uppercase letter (e.g., 43B)

Numeric part:

| Partition type | Partition | EP value | Boundary values | BVA test values |
|-|-:|-:|-:|-:|
| Invalid | MIN to −1 | −5 | −1 | −2 −1 0 |
| Invalid | 0 | 0 | 0 | −1 0 1 |
| Valid | 1 to 999 | 500 | 1, 999 | 0 1 2 998 999 1000 |
| Invalid | 1000 to MAX | 5000 | 1000 | 999 1000 1001 |

Letter suffix:

| Partition type | Partition | Test value |
|-|-|-|
| Valid | No letter | `43` |
| Valid | One uppercase letter A–Z | `43B` |
| Invalid | Lowercase letter | `43b` |
| Invalid | Two letters | `43BC` |
| Invalid | Letter before the number | `B43` |
| Invalid | Non-ASCII uppercase letter (A8) | `43Æ` |
| Invalid | Leading zero (A9) | `043` |

BVA on the letter: `43@` ✘, `43A` ✔, `43Z` ✔, `43[` ✘. Both ends combined: `1A` ✔, `999Z` ✔, `0A` ✘, `1000A` ✘.

### 6.3 Floor

> Floor. Either "st" or a number from 1 to 99

| Partition type | Partition | EP value | Boundary values | BVA test values |
|-|-:|-:|-:|-:|
| Invalid | MIN to −1 | −3 | −1 | −2 −1 0 |
| Invalid | 0 | 0 | 0 | −1 0 1 |
| Valid | 1 to 99 | 50 | 1, 99 | 0 1 2 98 99 100 |
| Invalid | 100 to MAX | 150 | 100 | 99 100 101 |

| Partition type | Partition | Test value |
|-|-|-|
| Valid | Ground floor | `st` |
| Invalid | Other casing | `ST`, `St` |
| Invalid | Real Danish value not in the requirement | `kl` (basement) |
| Invalid | Number with period | `1.` |
| Invalid | Leading zero (A9) | `01` |
| Invalid | Empty | `""` |

Sampling: both `st` and numeric floors occur.

### 6.4 Door

> Door. "th", "mf", "tv", a number from 1 to 50, or a lowercase letter optionally followed by a dash, then followed by one to three numeric digits (e.g., c3, d-14)

There are three valid forms, each with its own partitions. Sampling must produce all three, and form 3 both with and without the dash.

**Form 1: side of the stairwell**

| Partition type | Partition | Test value |
|-|-|-|
| Valid | Keyword | `th`, `mf`, `tv` |
| Invalid | Other casing | `TH`, `Tv` |
| Invalid | Near miss | `tr`, `t` |
| Invalid | Empty | `""` |

**Form 2: number from 1 to 50**

| Partition type | Partition | EP value | Boundary values | BVA test values |
|-|-:|-:|-:|-:|
| Invalid | MIN to −1 | −7 | −1 | −2 −1 0 |
| Invalid | 0 | 0 | 0 | −1 0 1 |
| Valid | 1 to 50 | 25 | 1, 50 | 0 1 2 49 50 51 |
| Invalid | 51 to MAX | 75 | 51 | 50 51 52 |

Leading zero `05` ✘ (A9).

**Form 3: letter, optional dash, one to three digits**

| Part | ✔ | ✘ |
|-|-|-|
| Letter | `a3`, `b3`, `y3`, `z3`, `æ3`, `å3` (A8) | `` `3 ``, `{3` (ASCII neighbours), `C3`, `Æ3` (uppercase) |
| Dash | `c3` (none), `d-14` (one) | `d--14` (two), `d14-` (after the digits) |
| Digit count without dash | `c3`, `c33`, `c333` | `c`, `c3333` |
| Digit count with dash | `d-1`, `d-123` | `d-`, `d-1234` |

The requirement says "numeric *digits*" here, not "a number". So `c0` and `c007` are ✔, unlike form 2, where `0` and `05` are ✘.

### 6.5 Postal code and town

> Postal code and town. Randomly extracted from the provided database addresses.sql

| Partition type | Partition | Test value |
|-|-|-|
| Valid | (code, town) is one row of `postal_code` | `2100`, `København Ø` |
| Invalid | Code not in the table | `0000`, `9999` |
| Invalid | Code and town from different rows | `2100`, `Skagen` |
| Invalid | Code with 3 or 5 digits (BVA on length 4) | `210`, `21000` |
| Invalid | Town not byte-equal to the table | `KÃ¸benhavn Ø` (double-encoded UTF-8) |

BVA on the size of the source data (stub `IPostalCodeRepository`): 0 rows → error (A3); 1 row → always that row; 2 rows → both returned within 1,000 seeded draws. In the integration test against MySQL, every generated pair must exist in the table.

## 7. Mobile phone number

> Format: Eight numeric digits. It must start by some of the following digit combinations: 2, 30, 31, 40, 41, 42, 50, 51, 52, 53, 60, 61, 71, 81, 91, 92, 93, 342, 344-349, 356-357, 359, 362, 365-366, 389, 398, 431, 441, 462, 466, 468, 472, 474, 476, 478, 485-486, 488-489, 493-496, 498-499, 542-543, 545, 551-552, 556, 571-574, 577, 579, 584, 586-587, 589, 597-598, 627, 629, 641, 649, 658, 662-665, 667, 692-694, 697, 771-772, 782-783, 785-786, 788-789, 826-827, 829

### 7.1 Format

| Aspect | Test values |
|-|-|
| Length (BVA on 8) | `""` ✘, `2012345` ✘ (7), `20123456` ✔ (8), `201234567` ✘ (9) |
| Characters | `2012345a` ✘, `+4520123456` ✘, `20 12 34 56` ✘ |

### 7.2 Prefix partitions

| Partition type | Partition | Test value |
|-|-|-|
| Valid | One-digit prefix: `2` | `25555555` |
| Valid | Two-digit prefix: 30 31 40 41 42 50 51 52 53 60 61 71 81 91 92 93 | `30555555` |
| Valid | Three-digit prefix: 342 … 829 | `34255555` |
| Invalid | Starts with 0 or 1 | `05555555`, `15555555` |
| Invalid | Two digits that start no listed prefix | `70555555`, `94555555` |
| Invalid | Three digits next to a listed prefix | `34355555` |

### 7.3 Boundary values

No listed prefix is longer than three digits, so the first three digits alone decide validity. Of the 1,000 possible three-digit starts (000–999), 337 are valid. They form 53 continuous runs; for example, `2` and `30`–`31` together cover 200–319. 3-value BVA on the edges of every run gives **187 test values: 101 ✔ and 86 ✘** (Appendix A). Each value becomes a test number by appending `12345`: 343 → `34312345`.

Twenty invalid values sit alone between two valid runs: 343, 358, 399, 430, 467, 473, 475, 477, 487, 497, 544, 578, 585, 588, 599, 628, 666, 784, 787, 828.

**Seams.** The requirement lists `2` separately from `30`–`31`, and `498-499` separately from `50`–`53`. Each pair forms one continuous run, so BVA on the runs alone never tests where they meet. **299, 300, 499, 500 ✔** are added. They catch implementations that check one-, two- and three-digit prefixes in separate branches.

**Whole-number boundaries:** `19999999` ✘, `20000000` ✔, `20000001` ✔, `93999998` ✔, `93999999` ✔, `94000000` ✘.

Sampling: 1,000 seeded numbers, all ✔, with every prefix length (one, two and three digits) occurring.

## 8. Cross-field consistency

When an endpoint returns several fields, they must agree with each other:

- **C1:** CPR digits 1–6 equal the date of birth as `ddMMyy`.
- **C2:** The parity of the CPR's final digit matches the gender (section 5.3).
- **C3:** First name, last name and gender come from the same record (A2).

| Condition / action | R1 | R2 | R3 | R4 | R5 | R6 | R7 | R8 |
|-|-|-|-|-|-|-|-|-|
| C1 CPR date = DOB | Y | Y | Y | Y | N | N | N | N |
| C2 Parity = gender | Y | Y | N | N | Y | Y | N | N |
| C3 Same record | Y | N | Y | N | Y | N | Y | N |
| **Result** | ✔ | ✘ | ✘ | ✘ | ✘ | ✘ | ✘ | ✘ |

The consistency oracle is tested with one constructed person per rule. Generated persons must always satisfy R1.

Which conditions apply to which endpoint:

| Endpoint | Fields | C1 | C2 | C3 |
|-|-|:-:|:-:|:-:|
| `/api/cpr` | CPR | – | – | – |
| `/api/name-gender` | first name, last name, gender | – | – | ✔ |
| `/api/name-gender-dob` | + date of birth | – | – | ✔ |
| `/api/cpr-name-gender` | CPR, first name, last name, gender | – | ✔ | ✔ |
| `/api/cpr-name-gender-dob` | + date of birth | ✔ | ✔ | ✔ |
| `/api/address` | address | – | – | – |
| `/api/phone` | phone number | – | – | – |
| `/api/person` | all | ✔ | ✔ | ✔ |
| `/api/persons?count=N` | all, for *N* persons | ✔ | ✔ | ✔ |

Every field in every response must also pass its own section's oracle. Each row is one request in the Postman collection.

## 9. Assumptions

The requirements leave these points open. The tests above depend on them.

| ID | Assumption |
|-|-|
| A1 | An invalid `count` gives HTTP 400 with a JSON error message; the service method throws `ArgumentOutOfRangeException` |
| A2 | A person's first name, last name and gender come from the same record in `person-names.json` |
| A3 | An empty data source (no names, no postal codes) is an error (HTTP 500), not an empty or partial person |
| A4 | The date of birth lies in (today − 100 years, today]; "today" comes from the injected `TimeProvider` |
| A5 | Dates are returned as ISO `yyyy-MM-dd` |
| A6 | An undefined `Gender` value throws `ArgumentOutOfRangeException` |
| A7 | Street names use A–Z, a–z, Æ Ø Å æ ø å and spaces; they start with a letter and contain no digits or punctuation |
| A8 | "Uppercase letter" (house number) means A–Z, without Æ Ø Å; "lowercase letter" (door) means a–z or æ ø å |
| A9 | Numbers have no leading zeros (`043`, `01`, `05` are invalid). The digits in door form 3 are digits, not a number, so `c0` and `c007` are valid |

## 10. Covered by experience-based design instead

These cases do not follow from the requirements, so they are not part of this black-box analysis:

- Real CPR rules the assignment leaves out: the century digit (7th) and the modulus-11 check
- Duplicate persons within one bulk response
- UTF-8 town names surviving end to end (database → API → UI)
- Randomness and distribution, e.g. the gender balance over many persons

## Appendix A: phone prefix boundary values

Three-digit prefixes from the 3-value BVA on every valid run (section 7.3). Test number = prefix + `12345`.

| Valid run | ✔ test values | ✘ test values |
|-|-|-|
| 200–319 | 200 201 318 319 | 199 320 |
| 342 | 342 | 341 343 |
| 344–349 | 344 345 348 349 | 343 350 |
| 356–357 | 356 357 | 355 358 |
| 359 | 359 | 358 360 |
| 362 | 362 | 361 363 |
| 365–366 | 365 366 | 364 367 |
| 389 | 389 | 388 390 |
| 398 | 398 | 397 399 |
| 400–429 | 400 401 428 429 | 399 430 |
| 431 | 431 | 430 432 |
| 441 | 441 | 440 442 |
| 462 | 462 | 461 463 |
| 466 | 466 | 465 467 |
| 468 | 468 | 467 469 |
| 472 | 472 | 471 473 |
| 474 | 474 | 473 475 |
| 476 | 476 | 475 477 |
| 478 | 478 | 477 479 |
| 485–486 | 485 486 | 484 487 |
| 488–489 | 488 489 | 487 490 |
| 493–496 | 493 494 495 496 | 492 497 |
| 498–539 | 498 499 538 539 | 497 540 |
| 542–543 | 542 543 | 541 544 |
| 545 | 545 | 544 546 |
| 551–552 | 551 552 | 550 553 |
| 556 | 556 | 555 557 |
| 571–574 | 571 572 573 574 | 570 575 |
| 577 | 577 | 576 578 |
| 579 | 579 | 578 580 |
| 584 | 584 | 583 585 |
| 586–587 | 586 587 | 585 588 |
| 589 | 589 | 588 590 |
| 597–598 | 597 598 | 596 599 |
| 600–619 | 600 601 618 619 | 599 620 |
| 627 | 627 | 626 628 |
| 629 | 629 | 628 630 |
| 641 | 641 | 640 642 |
| 649 | 649 | 648 650 |
| 658 | 658 | 657 659 |
| 662–665 | 662 663 664 665 | 661 666 |
| 667 | 667 | 666 668 |
| 692–694 | 692 693 694 | 691 695 |
| 697 | 697 | 696 698 |
| 710–719 | 710 711 718 719 | 709 720 |
| 771–772 | 771 772 | 770 773 |
| 782–783 | 782 783 | 781 784 |
| 785–786 | 785 786 | 784 787 |
| 788–789 | 788 789 | 787 790 |
| 810–819 | 810 811 818 819 | 809 820 |
| 826–827 | 826 827 | 825 828 |
| 829 | 829 | 828 830 |
| 910–939 | 910 911 938 939 | 909 940 |
