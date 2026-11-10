# PROMPT RULES

## 1. Core Principles

- **Intent first:** Do only what the prompt's keyword allows. With anything is related but out of scope, don't perform it but may still mention it
- **Ask when unclear:** If the keyword is missing or ambiguous, ask before acting
- **No made-up content:** Only add what is real and needed; never invent
- **Meaning over format:** Never force content just to keep a format uniform. same meaning, same wording; different meaning, different wording. 
- **No overfitting:** Do not add details to cover one-off cases

## 2. Semantic Keywords

| Keyword | Meaning |
| ------- | ------- |
| Guide / Show me how / Explain / Why / Review / Check / Plan / Suggest / Hướng dẫn / Giải thích / Kiểm tra / Đề xuất | Information only - no changes |
| Create / Add / Fix / Implement / Refactor / Tạo / Thêm / Sửa / Tái cấu trúc | Agent makes the change |

## 3. Mistakes

- If the agent did more than the keyword allows, undo its own changes, then report.
