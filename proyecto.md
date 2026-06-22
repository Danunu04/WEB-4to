\#DanaPerelmuter #IvanUrso

\### G01 - Propósito

Sportio es un gimnasio que ofrece múltiples disciplinas deportivas, entre las que se encuentran el gimnasio con entrenador personalizado, natación, pilates, danza y spinning, entre otras. Los alumnos pueden inscribirse en una o varias actividades según sus intereses y necesidades.



Actualmente, la gestión del gimnasio se realiza de forma manual mediante papel y planillas físicas. Cuando un alumno se da de alta, la recepcionista registra sus datos en una hoja de cálculo. En cuanto a las rutinas personalizadas, cada entrenador las comunica por el medio que le resulte más cómodo, ya sea por Instagram, WhatsApp o incluso en papel impreso, lo que genera inconsistencias y falta de organización.



El objetivo del proyecto es desarrollar una aplicación web integral que contemple a todos los actores del gimnasio, desde el personal de recepción hasta los alumnos. La plataforma permitirá gestionar altas de usuarios, pagos, rutinas de entrenamiento, seguimiento del progreso y la inscripción a competencias, todo desde un único lugar centralizado.



Esta solución aportará un mayor control sobre la asistencia, los horarios, las rutinas y los pagos, mejorando los tiempos de gestión y reduciendo los errores y demoras que hoy existen con el sistema manual. En definitiva, se busca modernizar y optimizar el funcionamiento interno del gimnasio en beneficio tanto del personal como de los alumnos.



\### G02 - Descripcion funcional y alcance

\*\*RFN1: Gestion de asociacion y abono mensual\*\*

La empresa deportiva Sportio necesita optimizar el proceso de asociación y cobro del abono mensual de sus alumnos. Actualmente, ambos trámites se realizan de forma presencial y completamente analógica, lo que implica el uso de planillas en papel y la asistencia física a las oficinas administrativas del gimnasio.



Esta modalidad genera una alta concentración de personas en las oficinas durante los primeros días de cada mes, momento en el que los alumnos acuden a renovar sus abonos. Dicha situación provoca demoras, largas filas y una sobrecarga de trabajo para el personal administrativo, afectando la experiencia tanto de los empleados como de los usuarios.



La solución propuesta consiste en digitalizar este proceso a través de la aplicación web de Sportio, permitiendo que los alumnos puedan realizar su asociación y el pago del abono mensual de forma remota, sin necesidad de concurrir físicamente al gimnasio. Esto agilizaría los tiempos de gestión y distribuiría la carga administrativa de manera más uniforme a lo largo del mes.



\*\*PN1 - Gestión de Asociación y Abono Mensual\*\*

\*\*Gestión de Asociación\*\*



1\. El usuario ingresa por primera vez a la aplicación web de Sportio y crea una cuenta registrando un nombre de usuario y contraseña.

2\. El sistema solicita los datos personales del alumno a asociar. Atributos: Nombre, Apellido, DNI, Teléfono y Fecha de nacimiento.

3\. El usuario completa y confirma los datos del alumno ingresados en el formulario.

4\. El sistema informa el monto correspondiente al primer mes de membresía y solicita que se proceda con el pago para finalizar la asociación.

5\. El usuario realiza el pago del primer mes a través de la plataforma. Atributos: Monto, medio de pago.

6\. El sistema registra al alumno como miembro activo de Sportio y habilita el acceso a los servicios del gimnasio. Atributos: Estado del alumno, fecha de inicio, fecha de vencimiento del abono.



\---



\*\*Abono Mensual\*\*



1\. El usuario accede a la sección de abono mensual a través del menú de navegación de la aplicación.

2\. El sistema identifica el tipo de usuario que está operando y presenta la información correspondiente según el caso. Atributos: Tipo de usuario, alumnos asociados, fecha de vencimiento, estado del abono.

&#x20;   - \*\*Usuario alumno o familiar:\*\* El sistema muestra la fecha de vencimiento del abono del alumno o alumnos asociados a su cuenta. En caso de que el abono se encuentre vencido, se indica el monto a abonar.

&#x20;   - \*\*Usuario administrativo:\*\* El sistema habilita un buscador para localizar a un alumno dentro del sistema y verificar el estado de su abono. Atributos: Nombre, Apellido, DNI, estado del abono.

3\. El usuario procede a realizar el pago del mes de actividad correspondiente a través de la plataforma. Atributos: Monto, medio de pago, mes abonado.

4\. El sistema registra el pago y actualiza el estado del abono del alumno. Atributos: Estado del abono, nueva fecha de vencimiento.



\*\*Políticas de Programación\*\*



\*\*Seguridad\*\*



\*\*Login\*\*



La seguridad del sistema se gestionará mediante dos patrones de diseño principales:



1\. \*\*Singleton:\*\* Garantiza que exista una única instancia activa del sistema en todo momento, evitando inconsistencias por sesiones duplicadas o accesos paralelos no controlados.

2\. \*\*Composite:\*\* Provee un sistema de familias y permisos granular que permite activar o desactivar módulos específicos del sistema según el nivel de acceso del usuario, asegurando que cada actor solo pueda operar sobre las funcionalidades que le corresponden.



\### Políticas de programación

\---

\*\*Intentos de acceso\*\*



Al momento de iniciar sesión, el usuario dispondrá de tres intentos para ingresar sus credenciales correctamente. En caso de agotar los tres intentos sin éxito, la cuenta quedará bloqueada temporalmente. Para proceder con el desbloqueo, el sistema presentará una serie de preguntas de seguridad basadas en la información registrada en el sistema, por ejemplo:



\- \_"¿En qué año naciste?"\_

\- En caso de tener más de un alumno asociado, se preguntará si el usuario conoce a una persona determinada, pudiendo ser un alumno registrado u otro nombre aleatorio, con el fin de validar la identidad del usuario.



Una vez respondidas correctamente las preguntas de seguridad, el usuario será redirigido al proceso de cambio de contraseña.



\---



\*\*Cambio de contraseña\*\*



Al momento de establecer una nueva contraseña, el sistema verificará que la misma no haya sido utilizada previamente por el usuario, obligando al ingreso de una credencial nueva.



\---



\*\*Creación de usuario\*\*



\- El nombre de usuario debe ser único dentro del sistema, no permitiéndose duplicados.

\- La contraseña debe cumplir con los siguientes requisitos mínimos de seguridad: al menos 6 caracteres, una letra mayúscula y un carácter especial.



\---



\*\*Encriptación\*\*



\- \*\*Reversible:\*\* La tabla de alumnos se almacenará de forma encriptada de manera reversible, dado que sus datos son utilizados para la generación de las preguntas de seguridad y deben poder ser consultados internamente por el sistema.

\- \*\*Irreversible:\*\* Las contraseñas de los usuarios se almacenarán utilizando el algoritmo de hash SHA256, garantizando que no puedan ser recuperadas en texto plano bajo ninguna circunstancia.

