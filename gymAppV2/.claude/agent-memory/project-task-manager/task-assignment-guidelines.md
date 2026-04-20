---
name: task-assignment-guidelines
description: Guidelines for assigning tasks to Dana and Ivan based on their skills
type: feedback
---

## Task Assignment Guidelines

### For Ivan (Technical Developer)
**Rule:** Assign focused, specific tasks with clear step-by-step instructions and acceptance criteria.

**Why:** Ivan is scatterbrained and gets lost easily. He needs very clear, narrow tasks to stay on track and complete work successfully.

**How to apply:**
- Break down complex features into small, specific tasks
- Provide numbered steps that must be followed in order
- Include clear acceptance criteria that can be verified
- Avoid tasks involving design patterns, profiles, or architectural decisions
- Focus on: CRUD operations, database queries, simple business logic, basic event logging
- Task format: "Implement [specific feature] using [specific approach]. Steps: 1. [step 1], 2. [step 2], 3. [step 3]. Acceptance: [clear criteria]"

### For Dana (Services & Visual Developer)
**Rule:** Assign structured, detailed tasks that leverage her attention to detail and ability to handle complexity.

**Why:** Dana excels at detailed work, service layer implementation, and UI/UX design. She can handle more complex, multi-step tasks with clear structure.

**How to apply:**
- Assign tasks requiring attention to detail and structure
- Include design pattern implementations (Singleton, Composite)
- Assign UI/UX design and CSS styling work
- Focus on: Service layer, validation, visual components, user experience
- Task format: "Design and implement [feature/service] with [requirements]. Consider [aspects]. Deliverables: [specific outputs]"

### General Guidelines
- Always specify `rem` units for CSS tasks (never `px`)
- Consider ASP.NET Web Forms architecture
- Reference database schema when relevant
- Include event logging where appropriate
- Ensure tasks align with permission system requirements
- Validate task dependencies are logical
- When in doubt, give Ivan simpler tasks and Dana more complex ones

### Task Complexity Estimation
- **Low:** Straightforward implementation, clear requirements, minimal dependencies
- **Medium:** Some complexity, multiple steps, moderate dependencies
- **High:** Complex logic, multiple components, significant dependencies, requires architectural decisions

### Dependencies
- Identify which tasks must be completed before others can start
- Mark parallel tasks that can be done simultaneously
- Consider both technical dependencies and team member availability