# Engineering lessons (S .NET Engineer)

Read this before writing or changing code. Apply the gate in section 5 before you report.

## How to use this file

- These lessons come from a real feature (email OTP) that was delivered about 70% right. A human reviewed it twice and raised 10+ points. The rule that came out of it: **a pattern may be violated once; the same pattern must never be violated twice.**
- Every lesson is a **pattern**, not a rule about OTP, email or .NET. The "Example" lines are illustrations only. Apply the pattern to any feature, layer, language or tool.
- When a new situation looks like a lesson, follow the lesson even if the details differ.

---

## 1. Requirements and intent

### L1. Existing code is part of the requirement
Stubs, method signatures, route names, file names, folder layout and config keys carry decisions the owner already made. Read them first and derive "what is already decided".
- Do not change an existing contract (signature, route, response shape, file name, folder) unless asked. If you think it is wrong, challenge it first (L3).
- Example: a stub already declared that verifying a code returns the same result as sign-in. It was replaced with a different result type, and extra state was invented to support it.

### L2. Refactor is not change
Words such as merge, move, rename, extract, split, clean up and reorganize mean **behavior-preserving**.
- Before editing, write down the observable behaviors: schedule, timing, limits, retention, error cases, public API.
- After editing, compare. The difference must be zero, or explicitly approved by the user.
- Example: "merge the cleanup job into the service" was done by replacing a timed job with cleanup-on-demand, which silently changed the behavior.

### L3. Challenge with evidence, then follow the decision
If a request conflicts with official docs, framework conventions, security practice or common standards, say so **before** implementing.
- Cite the standard, explain the cost, offer an alternative, and let the user decide.
- Do not silently comply and do not silently deviate.
- If the user phrases it conditionally ("if best practice says X, do it"), evaluate the condition and then execute. A theoretical answer alone is not enough.
- After the user decides, follow the decision completely.
- Example: a hosted background job (singleton) cannot hold scoped dependencies; merging scheduling into a scoped feature service conflicts with the framework's lifetime model.

### L4. Do not build what was not asked
Speculative design and extra behavior are scope creep: extra tables or columns for "future flows", extra endpoints, extra lockouts, extra tokens.
- Build the requested behavior. Put ideas in the report as suggestions.
- If something is truly required for the correctness or safety of what was asked, build the minimal version **and** call it out at the top of the report so it is easy to remove. Prefer asking first.
- Example: verification-token columns were added for a reset flow nobody had asked for yet.

### L5. Ask at forks that are costly to reverse
Ask once, batched, with a recommended default, when:
- a public contract changes (routes, response shapes, DTOs),
- the data schema changes,
- responsibility could belong to more than one domain or layer,
- two reasonable readings of the request exist.

Do not ask what the repository already answers (L1).

---

## 2. Architecture and placement

### L6. Respect layers and project boundaries
Put each piece of code in the layer designated for it: contract vs implementation, repository vs service vs controller, composition root vs everything else.
- The composition root (startup project) only wires things; it does not host business or data-access implementations.
- If the right project does not exist, that is a design signal: ask, or create it following the solution's naming pattern. Never park code in the wrong layer.
- Example: a repository interface was implemented inside the startup project instead of an implementation project.

### L7. Domain ownership: no leaks
Each domain owns its data and its concerns. Do not store, generate or interpret another domain's artifacts (tokens, credentials, sessions) in your feature's model. Call that domain's public service instead.

### L8. Reuse before create; equal outcomes share one path
Search the codebase for an existing capability before writing a new one. If a new flow produces the same outcome as an existing flow, reuse its service, DTO and result shape so both behave identically.
- Example: "verify code" is another way to authenticate, so it returns the same tokens through the same token service as sign-in.

### L9. One feature, one vertical slice
A cohesive feature gets its own controller, service, DTOs and folder. Do not bolt it onto an unrelated existing controller, service or DTO file. If you find feature code living in the wrong unit, move it out.

### L10. Generic mechanism vs domain logic
Reusable, cross-cutting mechanisms go to the shared/common layer with a parameterized core (delegate or template method). The feature supplies only the part that varies.
- Rule of thumb: if the code does not mention your domain nouns, it probably belongs in common.
- Example: retry with backoff was embedded in an email dispatcher; it is generic and takes the operation as a parameter.

### L11. One abstraction level per method; helpers vs business rules
Classify every private method:
- **Low-level technical tools** (stateless, no business meaning, reusable: normalizing, hashing, rendering, parsing, mapping) go to a helper/util class.
- **High-level business rules** (policy decisions, "what to do for which case") stay in the service.

Service methods should read like a story at a single level of abstraction.

---

## 3. Code hygiene

### L12. No dead code and no test-only production code
Every public member needs a real production consumer. Tests verify behavior; they do not justify existence.
- Before finishing, find references for every member you added and remove the unreferenced ones.
- Remove leftovers of abandoned approaches: fields, columns, options, config keys, files, migrations.
- Example: a method existed only because a test called it.

### L13. Follow language and repository conventions
- One type per file, file named after the type. Namespaces match folders.
- Match neighbors for formatting, indentation, line endings and file encoding.
- When the repository is silent, follow the official language/framework guidelines.
- Example: two classes were placed in one file.

### L14. Preserve existing names and assets; design extension by data
Do not rename or move existing files because your use is narrower or wider. Make the variation point extensible through a mapping (case -> asset) so a new case means adding one entry, not renaming things.
- Example: a template file was renamed to something generic, when the right design was one template per purpose chosen by a mapping.

### L15. Organize by feature and cohesion
When you add two or more related files, group them in a folder named after the capability (for example `Email/`, `Jobs/`). Do not scatter them in generic buckets such as `Tools/`, `Utils/` or `Misc/`.

---

## 4. Delivery and communication

### L16. Done means the whole path is closed
Trace it end to end before saying "done":
contract -> implementation -> registration -> caller -> configuration -> persistence or migration -> tests/docs as appropriate.
- Every interface has an implementation, a registration and a caller.
- Example: an interface existed with no implementation in the expected layer.

### L17. Honest status, first line
Start the report with what is actually done and what is not.
- If edits fail or the environment is read-only, say so immediately and stop. Do not write a long plan that looks like a finished task.
- After editing, verify the edit landed (view or grep) before claiming it.
- Distinguish "done", "done but unverified" and "proposed".

### L18. Respect action boundaries
Explicit instructions about what **not** to do (for example "do not build or test") are binding until the user lifts them.
- Do not take actions with side effects outside the repository (creating databases, starting servers, installing tools, running migrations against the user's data) unless asked or approved.
- Default habits such as "verify by running" give way to explicit instructions. Say what remains unverified instead.

### L19. Understand the intent, not only the words
- If the user corrects themselves or reframes, do not redo the previous wrong thing; restate your understanding in one line when it is non-trivial.
- Answer a direct question directly first ("should X be separate?" -> yes/no with reasons), then act according to the user's condition.

---

## 5. Pre-delivery gate (run every time, before reporting)

Answer each item honestly. Any "no" means fix it first.

1. Did I read the existing code, signatures and conventions, and keep every existing contract unless approved? (L1)
2. If the task was a refactor, is behavior identical? (L2)
3. Did I challenge anything that conflicts with a documented standard, with evidence? (L3)
4. Is every line of behavior something that was asked? Is extra work removed or flagged on top of the report? (L4)
5. Did I ask about every costly-to-reverse fork instead of guessing? (L5)
6. Is each piece of code in the correct layer/project, with the composition root only wiring? (L6)
7. Does anything leak another domain's concerns? Did I reuse existing capabilities? (L7, L8)
8. Is the feature isolated in its own slice, and are generic mechanisms in the shared layer? (L9, L10)
9. Are private methods split correctly between helpers and business rules? (L11)
10. Does every new public member have a production consumer, and are leftovers removed? (L12)
11. One type per file, conventions matched, existing names preserved, files grouped by feature? (L13, L14, L15)
12. Is the whole path closed: contract, implementation, registration, caller, config, migration? (L16)
13. Did I verify that edits actually landed, and does the report open with the honest status? (L17)
14. Did I stay inside the action boundaries the user set? (L18)
15. **Reviewer test:** imagine a strict reviewer who has read this whole file. What would they flag? Fix that before reporting.

---

## 6. Case log (illustrations only, not the full scope)

| Review finding | Pattern |
|---|---|
| Repository interface not implemented where expected | L6, L16 |
| Another domain's token fields stored in the feature's entity | L7, L4 |
| Verify returned a new result type instead of the sign-in result | L1, L8 |
| Retry logic embedded in the dispatcher | L10 |
| A method used only by tests | L12 |
| "Merge" changed schedule behavior | L2 |
| Scheduling put in a scoped service on request, not challenged | L3 |
| Feature bolted onto the Auth controller, service and DTO file | L9 |
| Existing template renamed to a generic name | L14, L1 |
| Two classes in one file | L13 |
| Mixed low-level and business private methods in one class | L11 |
| Related email files scattered in a generic folder | L15 |
| Long plan shown after edit tools failed | L17 |
| Extra verification run created a database and started servers after the user said not to | L18 |
